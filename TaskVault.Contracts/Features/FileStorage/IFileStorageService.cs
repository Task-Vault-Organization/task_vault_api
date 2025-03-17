using Microsoft.AspNetCore.Http;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage;

public interface IFileStorageService
{
    Task<BaseApiResponse> UploadFileAsync(IFormFile file);
    Task<BaseApiFileResponse> DownloadFileAsync(string fileName);
}