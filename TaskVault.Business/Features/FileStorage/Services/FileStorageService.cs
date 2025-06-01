using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Helpers;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;
using Task = System.Threading.Tasks.Task;

namespace TaskVault.Business.Features.FileStorage.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IFileRepository _fileRepository;
    private readonly IRepository<FileType> _fileTypeRepository;
    private readonly IAmazonS3 _s3Client;
    private readonly AwsOptions _awsOptions;
    private readonly TaskVaultDevContext _dbContext;
    private readonly ILogger<FileStorageService> _logger;
    private readonly IMapper _mapper;
    private readonly IEntityValidator _entityValidator;
    private readonly IDirectoryEntryRepository _directoryEntryRepository;

    private const int MaxDegreeOfParallelism = 5;

    public FileStorageService(
        IExceptionHandlingService exceptionHandlingService,
        IAmazonS3 s3Client,
        IOptions<AwsOptions> awsOptions,
        IFileRepository fileRepository,
        IRepository<FileType> fileTypeRepository,
        TaskVaultDevContext dbContext,
        ILogger<FileStorageService> logger,
        IMapper mapper,
        IEntityValidator entityValidator, IDirectoryEntryRepository directoryEntryRepository)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _s3Client = s3Client;
        _fileRepository = fileRepository;
        _fileTypeRepository = fileTypeRepository;
        _awsOptions = awsOptions.Value;
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
        _entityValidator = entityValidator;
        _directoryEntryRepository = directoryEntryRepository;
    }

    public async Task<BaseApiResponse> UploadFileAsync(string userEmail, UploadFileDto uploadFileDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);

            Guid directoryId = uploadFileDto.DirectoryId;
            if (directoryId != Guid.Empty)
            {
                var foundDirectory = await _entityValidator.GetFileOrThrowAsync(directoryId);
                await _entityValidator.EnsureFileOwnerAsync(foundDirectory, user);
            }

            var originalFileName = Path.GetFileNameWithoutExtension(uploadFileDto.File.FileName);
            var fileExtension = Path.GetExtension(uploadFileDto.File.FileName);

            var directoryEntries = await _directoryEntryRepository.FindAsync(de =>
                de.UserId == user.Id && de.DirectoryId == directoryId);

            var entryList = directoryEntries as DirectoryEntry[] ?? directoryEntries.ToArray();

            foreach (var entry in entryList)
            {
                entry.Index++;
                await _directoryEntryRepository.UpdateAsync(entry, new object[] { entry.UserId, entry.DirectoryId, entry.FileId });
            }

            var existingFilesInSameDir = entryList
                .Select(de => de.File!)
                .Where(f => f.UploaderId == user.Id && f.Name.StartsWith(originalFileName))
                .ToList();

            var usedNames = new HashSet<string>(existingFilesInSameDir.Select(f => f.Name));
            var finalName = originalFileName + fileExtension;

            if (usedNames.Contains(finalName))
            {
                int counter = 1;
                while (usedNames.Contains($"{originalFileName}({counter}){fileExtension}"))
                {
                    counter++;
                }
                finalName = $"{originalFileName}({counter}){fileExtension}";
            }

            var fileId = Guid.NewGuid();
            using var stream = uploadFileDto.File.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _awsOptions.BucketName,
                Key = fileId.ToString(),
                InputStream = stream,
                ContentType = uploadFileDto.File.ContentType
            };

            await _s3Client.PutObjectAsync(request);

            var fileType = (await _fileTypeRepository.FindAsync(ft => ft.Name == uploadFileDto.File.ContentType)).FirstOrDefault();
            if (fileType == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "File type not found");

            var newFile = File.Create(fileId, uploadFileDto.File.Length, finalName, user.Id, DateTime.UtcNow, fileType.Id);
            newFile.Owners = new List<User> { user };

            var fileHistoryLog = FileHistoryLog.Create($"{user.Email} uploaded this file", _mapper.Map<GetUserDto>(user));
            FileHistoryHelper.AddFileHistoryLog(newFile, fileHistoryLog);

            await _fileRepository.AddAsync(newFile);

            var newDirectoryEntry = new DirectoryEntry
            {
                UserId = user.Id,
                DirectoryId = directoryId,
                FileId = newFile.Id,
                Index = 0
            };

            await _directoryEntryRepository.AddAsync(newDirectoryEntry);

            return BaseApiResponse.Create($"Successfully uploaded file as: {finalName}");
        }, "Error when uploading file");
    }

    public async Task<BaseApiFileResponse> DownloadFileAsync(Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            await _entityValidator.GetFileOrThrowAsync(fileId);

            var request = new GetObjectRequest
            {
                BucketName = _awsOptions.BucketName,
                Key = fileId.ToString()
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);

            return BaseApiFileResponse.Create(memoryStream, response.Headers["Content-Type"]);
        }, "Error when downloading file");
    }

    public async Task<BaseApiResponse> CreateDirectoryAsync(string userEmail, CreateDirectoryDto createDirectoryDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);

            if (string.Equals(createDirectoryDto.DirectoryName, "root", StringComparison.OrdinalIgnoreCase))
                throw new ServiceException(StatusCodes.Status400BadRequest, "Cannot create a directory named 'root'");

            Guid parentDirectoryId = createDirectoryDto.ParentDirectoryId;

            if (parentDirectoryId != Guid.Empty)
            {
                var foundParentDir = await _entityValidator.GetFileOrThrowAsync(parentDirectoryId);
                await _entityValidator.EnsureFileOwnerAsync(foundParentDir, user);
            }

            var baseName = createDirectoryDto.DirectoryName;

            var siblingEntries = await _directoryEntryRepository.FindAsync(de =>
                de.UserId == user.Id && de.DirectoryId == parentDirectoryId);

            var directoryEntries = siblingEntries as DirectoryEntry[] ?? siblingEntries.ToArray();
            var existingDirsInSameLevel = directoryEntries
                .Select(de => de.File!)
                .Where(f => f.IsDirectory && f.UploaderId == user.Id && f.Name.StartsWith(baseName))
                .ToList();

            foreach (var entry in directoryEntries)
            {
                entry.Index++;
                await _directoryEntryRepository.UpdateAsync(entry, new object[] { entry.UserId, entry.DirectoryId, entry.FileId });
            }

            var usedNames = new HashSet<string>(existingDirsInSameLevel.Select(f => f.Name));
            var finalName = baseName;

            if (usedNames.Contains(baseName))
            {
                int counter = 1;
                while (usedNames.Contains($"{baseName}({counter})"))
                {
                    counter++;
                }
                finalName = $"{baseName}({counter})";
            }

            var newFolder = File.Create(
                Guid.NewGuid(),
                size: 0,
                name: finalName,
                uploaderId: user.Id,
                uploadedAt: DateTime.UtcNow,
                fileTypeId: 8,
                isDirectory: true
            );

            newFolder.Owners = new List<User> { user };

            var fileHistoryLog = FileHistoryLog.Create($"{user.Email} created this directory", _mapper.Map<GetUserDto>(user));
            FileHistoryHelper.AddFileHistoryLog(newFolder, fileHistoryLog);

            await _fileRepository.AddAsync(newFolder);

            var newDirectoryEntry = new DirectoryEntry
            {
                UserId = user.Id,
                DirectoryId = parentDirectoryId,
                FileId = newFolder.Id,
                Index = 0
            };

            await _directoryEntryRepository.AddAsync(newDirectoryEntry);

            return BaseApiResponse.Create($"Successfully created directory: {finalName}");
        }, "Error when creating directory");
    }

    public async Task<BaseApiResponse> UpdateFileIndexAsync(string userEmail, UpdateFileIndexDto updateFileIndexDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(updateFileIndexDto.FileId);

            var directoryEntry = (await _directoryEntryRepository.FindAsync(de =>
                de.UserId == user.Id && de.FileId == file.Id)).FirstOrDefault();

            if (directoryEntry is null)
                throw new UnauthorizedAccessException("User does not have access to reorder this file");

            int oldIndex = directoryEntry.Index;
            int newIndex = updateFileIndexDto.NewIndex;

            if (newIndex == oldIndex)
                return BaseApiResponse.Create("No change in file index");

            var siblingEntries = await _directoryEntryRepository.FindAsync(de =>
                de.UserId == user.Id &&
                de.DirectoryId == directoryEntry.DirectoryId &&
                de.FileId != file.Id);

            var entryList = siblingEntries as DirectoryEntry[] ?? siblingEntries.ToArray();

            if (newIndex < oldIndex)
            {
                var toIncrement = entryList
                    .Where(de => de.Index >= newIndex && de.Index < oldIndex)
                    .ToList();

                foreach (var entry in toIncrement)
                {
                    entry.Index++;
                    await _directoryEntryRepository.UpdateAsync(entry, new object[] { entry.UserId, entry.DirectoryId, entry.FileId });
                }
            }
            else
            {
                var toDecrement = entryList
                    .Where(de => de.Index <= newIndex && de.Index > oldIndex)
                    .ToList();

                foreach (var entry in toDecrement)
                {
                    entry.Index--;
                    await _directoryEntryRepository.UpdateAsync(entry, new object[] { entry.UserId, entry.DirectoryId, entry.FileId });
                }
            }

            directoryEntry.Index = newIndex;
            await _directoryEntryRepository.UpdateAsync(directoryEntry, new object[] { directoryEntry.UserId, directoryEntry.DirectoryId, directoryEntry.FileId });

            return BaseApiResponse.Create("Successfully reordered files");
        }, "Error when reordering files");
    }

    public async Task<BaseApiResponse> DeleteFileAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(fileId);

            var isUploader = file.UploaderId == user.Id;
            var isOwner = file.Owners?.Any(o => o.Id == user.Id) ?? false;

            if (!isUploader && !isOwner)
                throw new UnauthorizedAccessException("You do not have permission to delete this file");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (isUploader)
                {
                    var allEntries = await _directoryEntryRepository.FindAsync(de => de.FileId == fileId);
                    var directoryEntries = allEntries as DirectoryEntry[] ?? allEntries.ToArray();
                    var groupedByDirectory = directoryEntries.GroupBy(de => de.DirectoryId);

                    foreach (var group in groupedByDirectory)
                    {
                        var deletedIndex = group.OrderBy(de => de.Index).First().Index;

                        var affectedEntries = await _directoryEntryRepository.FindAsync(de =>
                            de.DirectoryId == group.Key && de.Index > deletedIndex);

                        foreach (var entry in affectedEntries)
                        {
                            entry.Index--;
                            await _directoryEntryRepository.UpdateAsync(entry, new object[] { entry.UserId, entry.DirectoryId, entry.FileId });
                        }
                    }

                    foreach (var entry in directoryEntries)
                    {
                        await _directoryEntryRepository.RemoveAsync(entry);
                    }

                    if (file.IsDirectory)
                        await DeleteFolderInternalAsync(file);
                    else
                        await SafeDeleteFileAsync(file);
                }
                else
                {
                    var userEntries = await _directoryEntryRepository.FindAsync(de =>
                        de.UserId == user.Id && de.FileId == fileId);

                    foreach (var entry in userEntries)
                    {
                        var affectedSiblings = await _directoryEntryRepository.FindAsync(de =>
                            de.UserId == user.Id &&
                            de.DirectoryId == entry.DirectoryId &&
                            de.Index > entry.Index);

                        foreach (var sibling in affectedSiblings)
                        {
                            sibling.Index--;
                            await _directoryEntryRepository.UpdateAsync(sibling, new object[] { sibling.UserId, sibling.DirectoryId, sibling.FileId });
                        }

                        await _directoryEntryRepository.RemoveAsync(entry);
                    }

                    file.Owners = file.Owners?.Where(o => o.Id != user.Id).ToList();
                    await _fileRepository.UpdateAsync(file, file.Id);
                }

                await transaction.CommitAsync();
                return BaseApiResponse.Create("Successfully deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file or folder with ID {FileId}", fileId);
                await transaction.RollbackAsync();
                throw;
            }
        }, "Error when deleting file");
    }

    private readonly HashSet<Guid> _processedFiles = new();

    private async Task DeleteFolderInternalAsync(File folder)
    {
        var contents = (await _directoryEntryRepository.FindAsync(de =>
            de.UserId == folder.UploaderId && de.DirectoryId == folder.Id)).Select(de => de.Directory);
        var deleteTasks = new List<Task>();

        foreach (var item in contents)
        {
            if (item != null && item.IsDirectory)
                deleteTasks.Add(DeleteFolderInternalAsync(item));
            else if (item != null) deleteTasks.Add(SafeDeleteFileAsync(item));

            if (deleteTasks.Count >= MaxDegreeOfParallelism)
            {
                await Task.WhenAll(deleteTasks);
                deleteTasks.Clear();
            }
        }

        if (deleteTasks.Any())
            await Task.WhenAll(deleteTasks);

        await SafeDeleteFileAsync(folder);
    }

    private async Task SafeDeleteFileAsync(File file)
    {
        try
        {
            if (!file.IsDirectory)
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _awsOptions.BucketName,
                    Key = file.Id.ToString()
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                _logger.LogInformation("Deleted file from S3 with key {FileId}", file.Id);
            }

            await _fileRepository.RemoveAsync(file);
            _logger.LogInformation("Deleted file with ID {FileId}", file.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file with ID {FileId}", file.Id);
        }
    }
}