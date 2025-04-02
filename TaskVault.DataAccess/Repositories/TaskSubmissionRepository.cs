using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class TaskSubmissionRepository : Repository<TaskSubmission>, ITaskSubmissionRepository 
{
    public TaskSubmissionRepository(TaskVaultDevContext context, ILogger<Repository<TaskSubmission>> logger) : base(context, logger)
    {
    }
}