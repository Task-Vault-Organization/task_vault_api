using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class FileShareRequestRepository: Repository<FileShareRequest>, IFileShareRequestRepository
{
    private readonly TaskVaultDevContext _context;
    public FileShareRequestRepository(TaskVaultDevContext context, ILogger<Repository<FileShareRequest>> logger) : base(context, logger)
    {
        _context = context;
    }

    public override async Task<IEnumerable<FileShareRequest>> FindAsync(Expression<Func<FileShareRequest, bool>> predicate)
    {
        try
        {
            return await _context.FileShareRequests
                .Include(de => de.File)
                .Include(de => de.From)
                .Include(de => de.To)
                .Include(de => de.Status)
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