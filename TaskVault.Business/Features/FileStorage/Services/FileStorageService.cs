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
    private readonly FileUploadOptions _fileUploadOptions;
    private readonly IFileHelpersService _fileHelpersService;
    
    public FileStorageService(
        IExceptionHandlingService exceptionHandlingService,
        IAmazonS3 s3Client,
        IOptions<AwsOptions> awsOptions,
        IFileRepository fileRepository,
        IRepository<FileType> fileTypeRepository,
        TaskVaultDevContext dbContext,
        ILogger<FileStorageService> logger,
        IMapper mapper,
        IEntityValidator entityValidator, IDirectoryEntryRepository directoryEntryRepository, IOptions<FileUploadOptions> fileUploadOptions, IFileHelpersService fileHelpersService)
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
        _fileHelpersService = fileHelpersService;
        _fileUploadOptions = fileUploadOptions.Value;
    }

    public async Task<BaseApiResponse> UploadFileAsync(string userEmail, UploadFileDto uploadFileDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            await _entityValidator.EnsureUserCannotUploadMoreThanMaxFilesAtOnceAsync(uploadFileDto);
            
            var fileNameWarnings = new List<string>();
            
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            
            var directoryId = uploadFileDto.DirectoryId;
            var foundDirectory = await _entityValidator.GetFileOrThrowAsync(directoryId);
            _entityValidator.ValidateOwnership(foundDirectory, user);

            var existingEntries = (await _directoryEntryRepository.FindAsync(de =>
                de.UserId == user.Id && de.DirectoryId == directoryId)).ToList();

            foreach (var formFile in uploadFileDto.Files)
            {
                if (formFile.Length > _fileUploadOptions.MaxFileToUploadSize)
                {
                    fileNameWarnings.Add(formFile.FileName);
                    continue;
                }

                var fileId = Guid.NewGuid();

                await _fileHelpersService.UploadToS3Async(formFile, fileId);

                var fileType = await GetFileTypeOrThrowAsync(formFile.ContentType);
                var fileName = _fileHelpersService.GenerateUniqueFileName(formFile.FileName, existingEntries);
                var newFile = CreateFileEntity(fileId, formFile.Length, fileName, user.Id, fileType.Id);
                
                if (foundDirectory.Owners != null)
                {
                    newFile.Owners = new List<User>(foundDirectory.Owners);
                }
                else newFile.Owners = new List<User>() { user };
                
                var log = FileHistoryLog.Create($"{user.Email} uploaded this file", _mapper.Map<GetUserDto>(user));
                FileHistoryHelper.AddFileHistoryLog(newFile, log);

                await _fileRepository.AddAsync(newFile);
                foreach (var owner in newFile.Owners)
                {
                    await _fileHelpersService.CreateDirectoryEntryAsync(newFile, owner.Id, foundDirectory.Id);
                }
            }

            if (fileNameWarnings.Any())
            {
                var warning = $"Could not upload the following files: {string.Join(", ", fileNameWarnings)}, they exceed the maximum file size of {(int)(_fileUploadOptions.MaxFileToUploadSize / (1024.0 * 1024.0))} MB";

                if (fileNameWarnings.Count == uploadFileDto.Files.Count())
                    return BaseApiResponse.Create("Upload finished with warnings", [warning]);

                return BaseApiResponse.Create("Successfully uploaded files with warnings", [warning]);
            }

            return BaseApiResponse.Create("Successfully uploaded files");
        }, "Error when uploading files");
    }

    private async Task<FileType> GetFileTypeOrThrowAsync(string contentType)
    {
        var type = (await _fileTypeRepository.FindAsync(ft => ft.Name == contentType)).FirstOrDefault();
        if (type == null)
            throw new ServiceException(StatusCodes.Status404NotFound, "File type not found");
        return type;
    }

    private File CreateFileEntity(Guid id, long size, string name, Guid uploaderId, int fileTypeId)
    {
        return File.Create(id, size, name, uploaderId, DateTime.UtcNow, fileTypeId);
    }

    public async Task<BaseApiFileResponse> DownloadFileAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(fileId);

            var request = new GetObjectRequest
            {
                BucketName = _awsOptions.BucketName,
                Key = fileId.ToString()
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            await using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var contentType = response.Headers.ContentType;
            return BaseApiFileResponse.Create(memoryStream, contentType);
        }, "Error when downloading file");
    }

    public async Task<BaseApiResponse> CreateDirectoryAsync(string userEmail, CreateDirectoryDto createDirectoryDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            
            if (string.Equals(createDirectoryDto.DirectoryName, "root", StringComparison.OrdinalIgnoreCase))
                throw new ServiceException(StatusCodes.Status400BadRequest, "Cannot create a directory named 'root'");

            var parentDirectoryId = createDirectoryDto.ParentDirectoryId;

            var foundParentDir = await _entityValidator.GetFileOrThrowAsync(parentDirectoryId);
            _entityValidator.ValidateOwnership(foundParentDir, user);

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

            var finalName =
                _fileHelpersService.GenerateUniqueFileName(createDirectoryDto.DirectoryName, directoryEntries.ToList());

            var newFolder = File.Create(
                Guid.NewGuid(),
                size: 0,
                name: finalName,
                uploaderId: user.Id,
                uploadedAt: DateTime.UtcNow,
                fileTypeId: 8,
                isDirectory: true
            );

            if (foundParentDir.Owners != null)
            {
                newFolder.Owners = new List<User>(foundParentDir.Owners);
            }
            else newFolder.Owners = new List<User>() { user };

            var fileHistoryLog = FileHistoryLog.Create($"{user.Email} created this directory", _mapper.Map<GetUserDto>(user));
            FileHistoryHelper.AddFileHistoryLog(newFolder, fileHistoryLog);

            await _fileRepository.AddAsync(newFolder);
            
            if (newFolder.Owners != null)
                foreach (var owner in newFolder.Owners)
                {
                    await _fileHelpersService.CreateDirectoryEntryAsync(newFolder, owner.Id, foundParentDir.Id);
                }

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
            _entityValidator.ValidateOwnership(file, user);

            var isUploader = file.UploaderId == user.Id;

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
                        var userGroups = group.GroupBy(de => de.UserId);
                        foreach (var userGroup in userGroups)
                        {
                            var deletedEntry = userGroup.FirstOrDefault(de => de.FileId == fileId);
                            if (deletedEntry == null) continue;

                            var deletedIndex = deletedEntry.Index;

                            var affectedEntries = await _directoryEntryRepository.FindAsync(de =>
                                de.DirectoryId == group.Key &&
                                de.UserId == userGroup.Key &&
                                de.Index > deletedIndex);

                            foreach (var entry in affectedEntries)
                            {
                                entry.Index--;
                                await _directoryEntryRepository.UpdateAsync(entry, new object[] { entry.UserId, entry.DirectoryId, entry.FileId });
                            }
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

    private async Task DeleteFolderInternalAsync(File rootFolder)
    {
        var stack = new Stack<File>();
        var foldersToDelete = new List<File>();
        stack.Push(rootFolder);

        while (stack.Count > 0)
        {
            var currentFolder = stack.Pop();
            foldersToDelete.Add(currentFolder);

            var entries = await _directoryEntryRepository.FindAsync(de =>
                de.UserId == currentFolder.UploaderId && de.DirectoryId == currentFolder.Id);

            foreach (var entry in entries)
            {
                var file = entry.File;
                if (file == null) continue;

                await _directoryEntryRepository.RemoveAsync(entry);

                if (file.IsDirectory)
                {
                    stack.Push(file);
                }
                else
                {
                    await SafeDeleteFileAsync(file);
                }
            }
        }

        foreach (var folder in foldersToDelete.AsEnumerable().Reverse())
        {
            await SafeDeleteFileAsync(folder);
        }
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
            throw;
        }
    }

    private async Task CreateDirectoryEntryAsync(File file, Guid userId, Guid directoryId)
    {
        try
        { 
            var newDirectoryEntry = DirectoryEntry.Create(userId, directoryId, file.Id, 0);
            var siblings = await _directoryEntryRepository.FindAsync(de =>
                de.UserId == userId &&
                de.DirectoryId == directoryId);

            var directoryEntries = siblings as DirectoryEntry[] ?? siblings.ToArray();
            foreach (var directoryEntry in directoryEntries)
            {
                directoryEntry.Index++;
            }
            
            await Task.WhenAll(directoryEntries.Select(entry =>
                _directoryEntryRepository.UpdateAsync(entry, new object[] { entry.UserId, entry.DirectoryId, entry.FileId })
            ));

            await _directoryEntryRepository.AddAsync(newDirectoryEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory entry");
            throw;
        }
    }
}