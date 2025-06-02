using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class NotificationsTypeRepository : Repository<NotificationType>, INotificationsTypeRepository
{
    public NotificationsTypeRepository(TaskVaultDevContext context, ILogger<Repository<NotificationType>> logger) : base(context, logger)
    {
    }
}