using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class FileShareRequestStatusRepository : Repository<FileShareRequestStatus>, IFileShareRequestStatusRepository
{
    public FileShareRequestStatusRepository(TaskVaultDevContext context, ILogger<Repository<FileShareRequestStatus>> logger) : base(context, logger)
    {
    }
}