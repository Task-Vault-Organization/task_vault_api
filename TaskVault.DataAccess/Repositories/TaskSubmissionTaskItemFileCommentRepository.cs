using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class TaskSubmissionTaskItemFileCommentRepository : Repository<TaskSubmissionTaskItemFileComment>, ITaskSubmissionTaskItemFileCommentRepository
{
    public TaskSubmissionTaskItemFileCommentRepository(TaskVaultDevContext context, ILogger<Repository<TaskSubmissionTaskItemFileComment>> logger)
        : base(context, logger)
    {
    }

    public override async Task<TaskSubmissionTaskItemFileComment?> GetByIdAsync<TId>(TId id)
    {
        try
        {
            return await Context.TaskSubmissionTaskItemFileComments
                .Include(c => c.TaskSubmission)
                    .ThenInclude(ts => ts.Task)
                .Include(c => c.TaskSubmissionTaskItemFile)
                .Include(c => c.FromUser)
                .FirstOrDefaultAsync(c => c.Id!.Equals(id));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving TaskSubmissionTaskItemFileComment with ID: {Id}", id);
            throw;
        }
    }

    public override async Task<IEnumerable<TaskSubmissionTaskItemFileComment>> FindAsync(Expression<Func<TaskSubmissionTaskItemFileComment, bool>> predicate)
    {
        try
        {
            return await Context.TaskSubmissionTaskItemFileComments
                .Include(c => c.TaskSubmission)
                    .ThenInclude(ts => ts.Task)
                .Include(c => c.TaskSubmissionTaskItemFile)
                .Include(c => c.FromUser)
                .Where(predicate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error finding TaskSubmissionTaskItemFileComments");
            throw;
        }
    }

    public override async Task<IEnumerable<TaskSubmissionTaskItemFileComment>> GetAllAsync()
    {
        try
        {
            return await Context.TaskSubmissionTaskItemFileComments
                .Include(c => c.TaskSubmission)
                    .ThenInclude(ts => ts.Task)
                .Include(c => c.TaskSubmissionTaskItemFile)
                .Include(c => c.FromUser)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving all TaskSubmissionTaskItemFileComments");
            throw;
        }
    }
}