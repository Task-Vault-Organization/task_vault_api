using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Abstractions;

public interface IFileService
{
    Task<GetFilesResponseDto> GetFilesSharedWithUserAsync(string userEmail);
    Task<GetFilesResponseDto> GetUploadedFilesByUserAsync(string userEmail);
    Task<GetFilesResponseDto> GetAllUserFilesAsync(string userEmail);
    Task<GetFileResponseDto> GetFileAsync(string userEmail, Guid fileId);
    Task<GetFileTypeReponseDto> GetAllFileTypesAsync(string userEmail);
    Task<GetFileCategoriesResponseDto> GetAllFileCategoriesAsync(string userEmail);
    Task<GetFilesResponseDto> GetAllDirectoryFilesAsync(string userEmail, Guid directoryId);
    Task<GetFilesResponseDto> GetUploadedFilesInDirectoryAsync(string userEmail, Guid directoryId);
    Task<GetFilesResponseDto> GetSharedFilesInDirectoryAsync(string userEmail, Guid directoryId);
    Task<RenameFileResponseDto> RenameFileAsync(string userEmail, RenameFileDto renameFileDto);
    Task<GetFileHistoryResponseDto> GetFileHistoryAsync(string userEmail, Guid fileId);
    Task<BaseApiResponse> MoveFileToDirectoryAsync(string userEmail, MoveFileToDirectoryDto moveFileToDirectoryDto);
}