using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.DataAccess.Repositories;

public class FileRepository : Repository<File>
{
    public FileRepository(TaskVaultDevContext context, ILogger<Repository<File>> logger) : base(context, logger)
    {
    }
    
    public override async Task<File?> GetByIdAsync<TId>(TId id)
    {
        try
        {
            return await Context.Files.Include((f) => f.FileType).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }
}