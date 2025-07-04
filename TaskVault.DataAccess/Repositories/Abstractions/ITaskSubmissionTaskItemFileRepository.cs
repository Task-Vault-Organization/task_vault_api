﻿using TaskVault.DataAccess.Entities;

namespace TaskVault.DataAccess.Repositories.Abstractions;

public interface ITaskSubmissionTaskItemFileRepository : IRepository<TaskSubmissionTaskItemFile>
{
    Task<TaskSubmissionTaskItemFile?> GetByIdWithFileAsync(Guid id);
}