using Microsoft.AspNetCore.Http;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Abstractions;

public interface IFileStorageService
{
    Task<BaseApiResponse> UploadFileAsync(string userEmail, UploadFileDto uploadFileDto);
    Task<BaseApiFileResponse> DownloadFileAsync(Guid fileId);
    Task<BaseApiResponse> CreateDirectoryAsync(string userEmail, CreateDirectoryDto createDirectoryDto);
    Task<BaseApiResponse> UpdateFileIndexAsync(string userEmail, UpdateFileIndexDto updateFileIndexDto);
}