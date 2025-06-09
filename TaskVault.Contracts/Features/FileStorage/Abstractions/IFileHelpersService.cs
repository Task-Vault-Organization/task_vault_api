using Microsoft.AspNetCore.Http;
using DirectoryEntry = TaskVault.DataAccess.Entities.DirectoryEntry;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.Contracts.Features.FileStorage.Abstractions;

public interface IFileHelpersService
{
    Task CreateDirectoryEntryAsync(File file, Guid userId, Guid directoryId);
    Task DeleteDirectoryEntryAsync(File file, Guid userId, Guid directoryId);
    string GenerateUniqueFileName(string originalName, List<DirectoryEntry> existingEntries);
    Task UploadToS3Async(IFormFile formFile, Guid fileId);
}