using Microsoft.Extensions.Logging;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess.Repositories;

public class EmailConfirmationRequestRepository : Repository<EmailConfirmationRequest>, IEmailConfirmationRequestRepository
{
    public EmailConfirmationRequestRepository(TaskVaultDevContext context, ILogger<Repository<EmailConfirmationRequest>> logger) : base(context, logger)
    {
    }
}