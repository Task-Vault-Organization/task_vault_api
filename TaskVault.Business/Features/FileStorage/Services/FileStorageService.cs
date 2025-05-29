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
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;
using Task = System.Threading.Tasks.Task;

namespace TaskVault.Business.Features.FileStorage.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IRepository<User> _userRepository;
    private readonly IFileRepository _fileRepository;
    private readonly IRepository<FileType> _fileTypeRepository;
    private readonly IAmazonS3 _s3Client;
    private readonly AwsOptions _awsOptions;
    private readonly TaskVaultDevContext _dbContext;
    private readonly ILogger<FileStorageService> _logger;
    private readonly IMapper _mapper;

    private const int MaxDegreeOfParallelism = 5;

    public FileStorageService(
        IExceptionHandlingService exceptionHandlingService,
        IAmazonS3 s3Client,
        IOptions<AwsOptions> awsOptions,
        IRepository<User> userRepository,
        IFileRepository fileRepository,
        IRepository<FileType> fileTypeRepository,
        TaskVaultDevContext dbContext,
        ILogger<FileStorageService> logger, IMapper mapper)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _s3Client = s3Client;
        _userRepository = userRepository;
        _fileRepository = fileRepository;
        _fileTypeRepository = fileTypeRepository;
        _awsOptions = awsOptions.Value;
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<BaseApiResponse> UploadFileAsync(string userEmail, UploadFileDto uploadFileDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");

            var originalFileName = Path.GetFileNameWithoutExtension(uploadFileDto.File.FileName);
            var fileExtension = Path.GetExtension(uploadFileDto.File.FileName);

            var existingFilesInSameDir = await _fileRepository.FindAsync(f =>
                !f.IsDirectory &&
                f.UploaderId == foundUser.Id &&
                f.DirectoryId == uploadFileDto.DirectoryId &&
                f.Name.StartsWith(originalFileName));

            var filesInDirectory = await _fileRepository.FindAsync(f =>
                f.UploaderId == foundUser.Id &&
                f.DirectoryId == uploadFileDto.DirectoryId);

            foreach (var file in filesInDirectory)
            {
                file.Index++;
                await _fileRepository.UpdateAsync(file, file.Id);
            }

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

            var newFile = File.Create(fileId, uploadFileDto.File.Length, finalName, foundUser.Id, DateTime.UtcNow, fileType.Id, 0);
            newFile.Owners = new List<User> { foundUser };
            newFile.DirectoryId = uploadFileDto.DirectoryId;

            var fileHistoryLog = FileHistoryLog.Create($"{foundUser.Email} uploaded this file",
                _mapper.Map<GetUserDto>(foundUser));

            FileHistoryHelper.AddFileHistoryLog(newFile, fileHistoryLog);

            await _fileRepository.AddAsync(newFile);
            return BaseApiResponse.Create($"Successfully uploaded file as: {finalName}");
        }, "Error when uploading file");
    }

    public async Task<BaseApiFileResponse> DownloadFileAsync(Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundDatabaseFIle = await _fileRepository.GetByIdAsync(fileId);
            if (foundDatabaseFIle == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "File not found");

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
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");

            var baseName = createDirectoryDto.DirectoryName;
            if (string.Equals(baseName, "root", StringComparison.OrdinalIgnoreCase))
                throw new ServiceException(StatusCodes.Status400BadRequest, "Cannot create a directory named 'root'");

            var filesInDirectory = await _fileRepository.FindAsync(f =>
                f.UploaderId == foundUser.Id &&
                f.DirectoryId == createDirectoryDto.ParentDirectoryId);

            foreach (var file in filesInDirectory)
            {
                file.Index++;
                await _fileRepository.UpdateAsync(file, file.Id);
            }

            var existingDirsInSameLevel = await _fileRepository.FindAsync(f =>
                f.IsDirectory &&
                f.UploaderId == foundUser.Id &&
                f.DirectoryId == createDirectoryDto.ParentDirectoryId &&
                f.Name.StartsWith(baseName));

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

            var newFolder = File.Create(Guid.NewGuid(), 0, finalName, foundUser.Id, DateTime.UtcNow, 8, 0, createDirectoryDto.ParentDirectoryId, true);
            newFolder.Owners = new List<User> { foundUser };

            var fileHistoryLog = FileHistoryLog.Create($"{foundUser.Email} created this directory",
                _mapper.Map<GetUserDto>(foundUser));

            FileHistoryHelper.AddFileHistoryLog(newFolder, fileHistoryLog);

            await _fileRepository.AddAsync(newFolder);
            return BaseApiResponse.Create($"Successfully created directory: {finalName}");
        }, "Error when creating directory");
    }

    public async Task<BaseApiResponse> UpdateFileIndexAsync(string userEmail, UpdateFileIndexDto updateFileIndexDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");

            var foundFile = await _fileRepository.GetFileByIdAsync(updateFileIndexDto.FileId);
            if (foundFile == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "File not found");

            int oldIndex = foundFile.Index;
            int newIndex = updateFileIndexDto.NewIndex;

            if (newIndex == oldIndex)
                return BaseApiResponse.Create("No change in file index");

            var siblingFiles = await _fileRepository.FindAsync(f =>
                f.DirectoryId == foundFile.DirectoryId && f.Id != foundFile.Id);

            if (newIndex < oldIndex)
            {
                var filesToIncrement = siblingFiles.Where(f => f.Index >= newIndex && f.Index < oldIndex).ToList();
                foreach (var file in filesToIncrement)
                {
                    file.Index++;
                    await _fileRepository.UpdateAsync(file, file.Id);
                }
            }
            else
            {
                var filesToDecrement = siblingFiles.Where(f => f.Index <= newIndex && f.Index > oldIndex).ToList();
                foreach (var file in filesToDecrement)
                {
                    file.Index--;
                    await _fileRepository.UpdateAsync(file, file.Id);
                }
            }

            foundFile.Index = newIndex;
            await _fileRepository.UpdateAsync(foundFile, foundFile.Id);
            return BaseApiResponse.Create("Successfully reordered files");
        }, "Error when reordering files");
    }

    public async Task<BaseApiResponse> DeleteFileAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");

            var foundFile = await _fileRepository.GetFileByIdAsync(fileId);
            if (foundFile == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "File not found");

            var directoryId = foundFile.DirectoryId;
            var deletedFileIndex = foundFile.Index;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (foundFile.IsDirectory)
                    await DeleteFolderInternalAsync(foundFile);
                else
                    await SafeDeleteFileAsync(foundFile);

                await AdjustSiblingIndexesAfterDeletionAsync(directoryId, deletedFileIndex);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file or folder with ID {FileId}", fileId);
                await transaction.RollbackAsync();
                throw;
            }

            return BaseApiResponse.Create("Successfully deleted");
        }, "Error when deleting file");
    }

    private async Task DeleteFolderInternalAsync(File folder)
    {
        var contents = await _fileRepository.FindAsync(f => f.DirectoryId == folder.Id);
        var deleteTasks = new List<Task>();

        foreach (var item in contents)
        {
            if (item.IsDirectory)
                deleteTasks.Add(DeleteFolderInternalAsync(item));
            else
                deleteTasks.Add(SafeDeleteFileAsync(item));

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

    private async Task AdjustSiblingIndexesAfterDeletionAsync(Guid? directoryId, int deletedIndex)
    {
        var siblings = await _fileRepository.FindAsync(f =>
            f.DirectoryId == directoryId && f.Index > deletedIndex);

        foreach (var file in siblings)
        {
            file.Index--;
            await _fileRepository.UpdateAsync(file, file.Id);
        }
    }
}