using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
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

    public override async Task<IEnumerable<TaskSubmissionTaskItemFile>> FindAsync(Expression<Func<TaskSubmissionTaskItemFile, bool>> predicate)
    {
        try
        {
            return await Context.TaskSubmissionTaskItemFiles
                .Include(c => c.File)
                .Include(c => c.Comments)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving all TaskSubmissionTaskItemFileComments");
            throw;
        }
    }
}