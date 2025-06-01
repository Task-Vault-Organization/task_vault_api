using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.DataAccess.Repositories.Abstractions;

public interface IFileRepository : IRepository<File>
{
    Task<IEnumerable<File>> GetAllUploadedFilesAsync(Guid uploaderId);
    Task<IEnumerable<File>> GetAllSharedFilesAsync(Guid userId);
    Task<IEnumerable<File>> GetAllAccessibleFilesAsync(Guid userId);
    Task<File?> GetFileByIdAsync(Guid id);
    Task<IEnumerable<File>> GetAllFilesInDirectoryForUserAsync(Guid userId, Guid directoryId);
    Task<IEnumerable<File>> GetUploadedFilesInDirectoryAsync(Guid userId, Guid directoryId);
    Task<IEnumerable<File>> GetSharedFilesInDirectoryAsync(Guid userId, Guid directoryId);
}