using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.Business.Features.FileStorage.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IRepository<User> _userRepository;
    private readonly IFileRepository _fileRepository;
    private IRepository<FileType> _fileTypeRepository;
    private readonly IAmazonS3 _s3Client;
    private readonly AwsOptions _awsOptions;

    public FileStorageService(IExceptionHandlingService exceptionHandlingService, IAmazonS3 s3Client, IOptions<AwsOptions> awsOptions, IRepository<User> userRepository, IFileRepository fileRepository, IRepository<FileType> fileTypeRepository)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _s3Client = s3Client;
        _userRepository = userRepository;
        _fileRepository = fileRepository;
        _fileTypeRepository = fileTypeRepository;
        _awsOptions = awsOptions.Value;
    }

    public async Task<BaseApiResponse> UploadFileAsync(string userEmail, UploadFileDto uploadFileDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var originalFileName = Path.GetFileNameWithoutExtension(uploadFileDto.File.FileName);
            var fileExtension = Path.GetExtension(uploadFileDto.File.FileName);
            var directoryId = (Guid?)null;

            if (uploadFileDto.DirectoryName != "root")
            {
                var foundDirectory = (await _fileRepository.FindAsync(f =>
                    f.IsDirectory &&
                    f.Name == uploadFileDto.DirectoryName &&
                    f.UploaderId == foundUser.Id)).FirstOrDefault();

                if (foundDirectory == null)
                {
                    throw new ServiceException(StatusCodes.Status404NotFound, "Directory not found");
                }

                directoryId = foundDirectory.Id;
            }

            var existingFilesInSameDir = await _fileRepository.FindAsync(f =>
                !f.IsDirectory &&
                f.UploaderId == foundUser.Id &&
                f.DirectoryId == directoryId &&
                f.Name.StartsWith(originalFileName));

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
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "File type not found");
            }

            var newFile = File.Create(fileId, uploadFileDto.File.Length, finalName, foundUser.Id, DateTime.UtcNow, fileType.Id);
            newFile.Owners = new List<User> { foundUser };
            newFile.DirectoryId = directoryId;

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
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "File not found");
            }
            
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
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var baseName = createDirectoryDto.DirectoryName;
            if (string.Equals(baseName, "root", StringComparison.OrdinalIgnoreCase))
            {
                throw new ServiceException(StatusCodes.Status400BadRequest, "Cannot create a directory named 'root'");
            }

            Guid? parentDirectoryId = null;
            if (!string.IsNullOrEmpty(createDirectoryDto.ParentDirectoryName))
            {
                var parentDirectory = (await _fileRepository.FindAsync(f =>
                    f.IsDirectory &&
                    f.Name == createDirectoryDto.ParentDirectoryName &&
                    f.UploaderId == foundUser.Id)).FirstOrDefault();

                if (parentDirectory == null)
                {
                    throw new ServiceException(StatusCodes.Status404NotFound, "Parent directory not found");
                }

                parentDirectoryId = parentDirectory.Id;
            }

            var existingDirsInSameLevel = await _fileRepository.FindAsync(f =>
                f.IsDirectory &&
                f.UploaderId == foundUser.Id &&
                f.DirectoryId == parentDirectoryId &&
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

            var newFolder = File.Create(
                Guid.NewGuid(),
                0,
                finalName,
                foundUser.Id,
                DateTime.UtcNow,
                8,
                parentDirectoryId,
                true
            );

            await _fileRepository.AddAsync(newFolder);
            return BaseApiResponse.Create($"Successfully created directory: {finalName}");
        }, "Error when creating directory");
    }
}