using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class CustomFileCategoryRepository : Repository<CustomFileCategory>, ICustomFileCategoryRepository
{
    public CustomFileCategoryRepository(TaskVaultDevContext context, ILogger<Repository<CustomFileCategory>> logger) : base(context, logger)
    {
    }
}