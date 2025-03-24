using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.DataAccess.Repositories.Abstractions;

public interface IFileRepository : IRepository<File>
{
    Task<IEnumerable<File>> GetAllUploadedFilesAsync(Guid uploaderId);
    Task<IEnumerable<File>> GetAllSharedFilesAsync(Guid uploaderId);
}