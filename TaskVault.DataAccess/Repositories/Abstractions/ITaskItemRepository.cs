using TaskVault.DataAccess.Entities;

namespace TaskVault.DataAccess.Repositories.Abstractions;

public interface ITaskItemRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetTaskItemOfTaskAsync(Guid taskId);
}