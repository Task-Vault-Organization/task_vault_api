using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class TaskSubmissionRepository : Repository<TaskSubmission>, ITaskSubmissionRepository
{
    private readonly TaskVaultDevContext _context;
    public TaskSubmissionRepository(TaskVaultDevContext context, ILogger<Repository<TaskSubmission>> logger) : base(context, logger)
    {
        _context = context;
    }

    public async Task<TaskSubmission?> GetTaskSubmissionByIdAsync(Guid id)
    {
        try
        {
            return await Context.TaskSubmissions.Include(ts => ts.Task)
                .FirstOrDefaultAsync(ts => ts.Id == id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }
}