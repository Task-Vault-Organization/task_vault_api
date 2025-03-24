using Microsoft.AspNetCore.Http;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Abstractions;

public interface IFileStorageService
{
    Task<BaseApiResponse> UploadFileAsync(string userEmail, IFormFile file);
    Task<BaseApiFileResponse> DownloadFileAsync(string userEmail, Guid fileID);
}