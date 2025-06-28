using Microsoft.AspNetCore.Http;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Abstractions;

public interface IFileStorageService
{
    Task<UploadFileResponseDto> UploadFileAsync(string userEmail, UploadFileDto uploadFileDto);
    Task<BaseApiFileResponse> DownloadFileAsync(string userEmail, Guid fileId);
    Task<BaseApiResponse> CreateDirectoryAsync(string userEmail, CreateDirectoryDto createDirectoryDto);
    Task<BaseApiResponse> UpdateFileIndexAsync(string userEmail, UpdateFileIndexDto updateFileIndexDto);
    Task<BaseApiResponse> DeleteFileAsync(string userEmail, Guid fileId);
    Task<BaseApiResponse> CreateCustomFileCategoryAsync(string userEmail, CreateCustomFileCategoryDto dto);
    Task<BaseApiResponse> DeleteCustomFileCategoryAsync(string userEmail, int categoryId);
}