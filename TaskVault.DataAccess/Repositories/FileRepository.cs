using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.DataAccess.Repositories;

public class FileRepository : Repository<File>, IFileRepository
{
    public FileRepository(TaskVaultDevContext context, ILogger<Repository<File>> logger) : base(context, logger)
    {
    }
    
    public override async Task<File?> GetByIdAsync<TId>(TId id)
    {
        try
        {
            return await Context.Files
                .Include((f) => f.FileType)
                .Include(f => f.Owners)
                .Include(f => f.Uploader)
                .Include(f => f.Uploader)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }

    public async Task<IEnumerable<File>> GetAllUploadedFilesAsync(Guid uploaderId)
    {
        try
        {
            return await Context.Files
                .Include((f) => f.FileType)
                .Include(f => f.Owners)
                .Include(f => f.Uploader)
                .Where(f => f.UploaderId == uploaderId).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }

    public async Task<IEnumerable<File>> GetAllSharedFilesAsync(Guid uploaderId)
    {
        try
        {
            return await Context.Files
                .Include(f => f.FileType)
                .Include(f => f.Owners)
                .Include(f => f.Uploader)
                .Where(f => f.UploaderId != uploaderId
                            && f.Owners!.Any(u => u.Id == uploaderId))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching shared files for uploader: {UploaderId}", uploaderId);
            throw;
        }
    }
}