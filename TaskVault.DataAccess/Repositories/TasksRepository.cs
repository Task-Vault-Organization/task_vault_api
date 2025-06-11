using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Repositories.Abstractions;
using Task = TaskVault.DataAccess.Entities.Task;

namespace TaskVault.DataAccess.Repositories;

public class TasksRepository : Repository<Task>, ITasksRepository
{
    public TasksRepository(TaskVaultDevContext context, ILogger<Repository<Task>> logger) : base(context, logger)
    {
    }

    public async Task<IEnumerable<Task>> GetOwnedTasksAsync(Guid ownerId)
    {
        try
        {
            return await Context.Tasks.Include(t => t.Assignees)
                .Include(t => t.Owner)
                .Include(t => t.Status)
                .Where(t => t.OwnerId == ownerId).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }

    public async Task<IEnumerable<Task>> GetAssignedTasksAsync(Guid ownerId)
    {
        try
        {
            return await Context.Tasks.Include(t => t.Assignees)
                .Include(t => t.Owner)
                .Include(t => t.Status)
                .Include(t => t.TaskSubmissions)
                .Where(t => t.Assignees!.Any(a => a.Id == ownerId)).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }

    public async Task<Task?> GetTaskByIdAsync(Guid id)
    {
        try
        {
            return await Context.Tasks.Include(t => t.Assignees)
                .Include(t => t.Owner)
                .Include(t => t.Status)
                .Include(t => t.TaskSubmissions)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching task with ID: {TaskId}", id);
            throw;
        }
    }

}