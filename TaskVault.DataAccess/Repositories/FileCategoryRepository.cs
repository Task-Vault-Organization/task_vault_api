using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class FileCategoryRepository : Repository<FileCategory>, IFileCategoryRepository
{
    public FileCategoryRepository(TaskVaultDevContext context, ILogger<Repository<FileCategory>> logger) : base(context, logger)
    {
    }
}