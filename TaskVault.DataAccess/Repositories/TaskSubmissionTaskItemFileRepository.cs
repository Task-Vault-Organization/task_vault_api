using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class TaskSubmissionTaskItemFileRepository : Repository<TaskSubmissionTaskItemFile>, ITaskSubmissionTaskItemFileRepository
{
    public TaskSubmissionTaskItemFileRepository(TaskVaultDevContext context, ILogger<Repository<TaskSubmissionTaskItemFile>> logger) : base(context, logger)
    {
    }
}