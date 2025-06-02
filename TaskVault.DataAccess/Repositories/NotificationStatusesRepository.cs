using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class NotificationStatusesRepository : Repository<NotificationStatus>, INotificationsStatusesRepository
{
    public NotificationStatusesRepository(TaskVaultDevContext context, ILogger<Repository<NotificationStatus>> logger) : base(context, logger)
    {
    }
}