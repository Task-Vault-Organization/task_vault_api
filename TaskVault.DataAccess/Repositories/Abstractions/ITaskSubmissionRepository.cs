using TaskVault.DataAccess.Entities;

namespace TaskVault.DataAccess.Repositories.Abstractions;

public interface ITaskSubmissionRepository : IRepository<TaskSubmission>
{
    Task<TaskSubmission?> GetTaskSubmissionByIdAsync(Guid id);
}