using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class NotificationsRepository : Repository<Notification>, INotificationRepository
{
    private readonly TaskVaultDevContext _context;
    
    public NotificationsRepository(TaskVaultDevContext context, ILogger<Repository<Notification>> logger) : base(context, logger)
    {
        _context = context;
    }

    public override async Task<IEnumerable<Notification>> FindAsync(Expression<Func<Notification, bool>> predicate)
    {
        try
        {
            return await _context.Set<Notification>()
                .Include((n) => n.ToUser)
                .Include((n) => n.NotificationStatus)
                .Include((n) => n.NotificationType)
                .Where(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }

    public override async Task<IEnumerable<Notification>> GetAllAsync()
    {
        try
        {
            return await Context.Set<Notification>()
                .Include((n) => n.ToUser)
                .Include((n) => n.NotificationStatus)
                .Include((n) => n.NotificationType)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }

    public override async Task<Notification?> GetByIdAsync<TId>(TId id)
    {
        try
        {
            return await Context.Set<Notification>()
                .Include(n => n.ToUser)
                .Include(n => n.NotificationStatus)
                .Include(n => n.NotificationType)
                .FirstOrDefaultAsync(n => n.Id.Equals(id));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }
}