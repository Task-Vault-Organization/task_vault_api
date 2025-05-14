using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Abstractions;

public interface IFileService
{
    Task<GetUploadedFilesResponseDto> GetAllUploadedFilesAsync(string userEmail);
    Task<GetSharedFilesResponseDto> GetAllSharedFilesAsync(string userEmail);
    Task<GetFileResponseDto> GetFileAsync(string userEmail, Guid fileId);
    Task<BaseApiResponse> DeleteUploadedFileAsync(string userEmail, Guid fileId);
    Task<GetFileTypeReponseDto> GetAllFileTypesAsync(string userEmail);
    Task<GetFileCategoriesResponseDto> GetAllFileCategoriesAsync(string userEmail);
    Task<GetUploadedFilesResponseDto> GetAllUploadedDirectoryFilesAsync(string userEmail, string? directoryName);
}