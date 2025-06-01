using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using System.Linq.Expressions;
using Task = System.Threading.Tasks.Task;

namespace TaskVault.DataAccess.Repositories;

public class DirectoryEntryRepository : Repository<DirectoryEntry>, IDirectoryEntryRepository
{
    private readonly TaskVaultDevContext _context;

    public DirectoryEntryRepository(TaskVaultDevContext context, ILogger<Repository<DirectoryEntry>> logger) : base(context, logger)
    {
        _context = context;
    }

    public override async Task UpdateAsync<TId>(DirectoryEntry updatedItem, TId id, bool applyChanges = true)
    {
        DirectoryEntry? existingEntity = id switch
        {
            object[] compositeKey => await _context.Set<DirectoryEntry>().FindAsync(compositeKey),
            _ => await _context.Set<DirectoryEntry>().FindAsync(id)
        };

        if (existingEntity == null)
            throw new Exception("Entity not found");

        _context.Entry(existingEntity).CurrentValues.SetValues(updatedItem);

        if (applyChanges)
            await _context.SaveChangesAsync();
    }

    public override async Task<IEnumerable<DirectoryEntry>> FindAsync(Expression<Func<DirectoryEntry, bool>> predicate)
    {
        try
        {
            return await _context.DirectoryEntries
                .Include(de => de.File)
                .Where(predicate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }
}