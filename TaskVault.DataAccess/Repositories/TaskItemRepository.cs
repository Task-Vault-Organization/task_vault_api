using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class TaskItemRepository : Repository<TaskItem>, ITaskItemRepository
{
    public TaskItemRepository(TaskVaultDevContext context, ILogger<Repository<TaskItem>> logger) : base(context, logger)
    {
    }

    public async Task<IEnumerable<TaskItem>> GetTaskItemOfTaskAsync(Guid taskId)
    {
        try
        {
            return await Context.TaskItems.Include(ti => ti.FileType)
                .Include(ti => ti.FileCategory)
                .Where(ti => ti.TaskId == taskId).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }
}