using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.FileStorage;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Business.Features.FileStorage.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IAmazonS3 _s3Client;
    private readonly AwsOptions _awsOptions;

    public FileStorageService(IExceptionHandlingService exceptionHandlingService, IAmazonS3 s3Client, IOptions<AwsOptions> awsOptions)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _s3Client = s3Client;
        _awsOptions = awsOptions.Value;
    }

    public async Task<BaseApiResponse> UploadFileAsync(IFormFile file)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _awsOptions.BucketName,
                Key = file.FileName,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request);
            return BaseApiResponse.Create("Successfully uploaded file");
        }, "Error when uploading file");
    }

    public async Task<BaseApiFileResponse> DownloadFileAsync(string fileName)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var request = new GetObjectRequest
            {
                BucketName = _awsOptions.BucketName,
                Key = fileName
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);
            return BaseApiFileResponse.Create(memoryStream, response.Headers["Content-Type"]);
        }, "Error when downloading file");
    }
}