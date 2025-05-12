using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class FileTypeRepository : Repository<FileType>, IFileTypeRepository
{
    public FileTypeRepository(TaskVaultDevContext context, ILogger<Repository<FileType>> logger) : base(context, logger)
    {
    }
}