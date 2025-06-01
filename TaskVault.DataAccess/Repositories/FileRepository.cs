using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.DataAccess.Repositories;

public class FileRepository : Repository<File>, IFileRepository
{
    public FileRepository(TaskVaultDevContext context, ILogger<Repository<File>> logger) 
        : base(context, logger)
    {
    }

    public override async Task<File?> GetByIdAsync<TId>(TId id)
    {
        try
        {
            return await Context.Files
                .Include(f => f.FileType)
                .Include(f => f.Owners)
                .Include(f => f.Uploader)
                .FirstOrDefaultAsync(f => f.Id!.Equals(id));
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
                .Include(f => f.FileType)
                .Include(f => f.Owners)
                .Include(f => f.Uploader)
                .Where(f => f.UploaderId == uploaderId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }

    public async Task<IEnumerable<File>> GetAllSharedFilesAsync(Guid userId)
    {
        try
        {
            return await Context.DirectoryEntries
                .Include(de => de.File)
                    .ThenInclude(f => f.FileType)
                .Include(de => de.File)
                    .ThenInclude(f => f.Uploader)
                .Where(de => de.UserId == userId && de.File!.UploaderId != userId)
                .Select(de => de.File!)
                .Distinct()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching shared files for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<File>> GetAllAccessibleFilesAsync(Guid userId)
    {
        try
        {
            var uploadedFiles = await GetAllUploadedFilesAsync(userId);
            var sharedFiles = await GetAllSharedFilesAsync(userId);
            return uploadedFiles.Concat(sharedFiles).DistinctBy(f => f.Id).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching accessible files for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<File?> GetFileByIdAsync(Guid id)
    {
        try
        {
            return await Context.Files
                .Include(f => f.FileType)
                .Include(f => f.Owners)
                .Include(f => f.Uploader)
                .FirstOrDefaultAsync(f => f.Id == id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }

    public async Task<IEnumerable<File>> GetAllFilesInDirectoryForUserAsync(Guid userId, Guid directoryId)
    {
        return await Context.DirectoryEntries
            .Include(de => de.File)
                .ThenInclude(f => f.FileType)
            .Include(de => de.File)
                .ThenInclude(f => f.Uploader)
            .Include(de => de.File)
                .ThenInclude(f => f.Owners)
            .Where(de => de.UserId == userId && de.DirectoryId == directoryId)
            .OrderBy(de => de.Index)
            .Select(de => de.File!)
            .ToListAsync();
    }

    public async Task<IEnumerable<File>> GetUploadedFilesInDirectoryAsync(Guid userId, Guid directoryId)
    {
        return await Context.DirectoryEntries
            .Include(de => de.File)
                .ThenInclude(f => f.FileType)
            .Include(de => de.File)
                .ThenInclude(f => f.Uploader)
            .Include(de => de.File)
                .ThenInclude(f => f.Owners)
            .Where(de => de.UserId == userId 
                         && de.DirectoryId == directoryId 
                         && de.File!.UploaderId == userId)
            .OrderBy(de => de.Index)
            .Select(de => de.File!)
            .ToListAsync();
    }

    public async Task<IEnumerable<File>> GetSharedFilesInDirectoryAsync(Guid userId, Guid directoryId)
    {
        return await Context.DirectoryEntries
            .Include(de => de.File)
                .ThenInclude(f => f.FileType)
            .Include(de => de.File)
                .ThenInclude(f => f.Uploader)
            .Include(de => de.File)
                .ThenInclude(f => f.Owners)
            .Where(de => de.UserId == userId 
                         && de.DirectoryId == directoryId 
                         && de.File!.UploaderId != userId)
            .OrderBy(de => de.Index)
            .Select(de => de.File!)
            .ToListAsync();
    }
}