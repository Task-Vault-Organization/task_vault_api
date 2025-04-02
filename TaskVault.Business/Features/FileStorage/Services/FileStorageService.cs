using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
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

    public async Task<BaseApiResponse> UploadFileAsync(string userEmail, IFormFile file)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var fileId = Guid.NewGuid();

            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _awsOptions.BucketName,
                Key = fileId.ToString(),
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request);

            var fileType = (await _fileTypeRepository.FindAsync((ft) => ft.Name == file.ContentType))
                .FirstOrDefault();

            if (fileType == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "File type not found");
            }

            var newFile = File.Create(fileId, file.Length, file.FileName, foundUser.Id, DateTime.UtcNow,
                fileType.Id);
            var owners = new List<User>() { foundUser };
            newFile.Owners = owners;

            await _fileRepository.AddAsync(newFile);

            return BaseApiResponse.Create("Successfully uploaded file and saved metadata");
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
}