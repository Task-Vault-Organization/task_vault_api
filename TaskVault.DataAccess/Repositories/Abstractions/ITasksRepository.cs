using Task = TaskVault.DataAccess.Entities.Task;

namespace TaskVault.DataAccess.Repositories.Abstractions;

public interface ITasksRepository : IRepository<Task>
{
    Task<IEnumerable<Task>> GetOwnedTasksAsync(Guid ownerId);
    Task<IEnumerable<Task>> GetAssignedTasksAsync(Guid ownerId);
    Task<Task?> GetTaskByIdAsync(Guid id);
}