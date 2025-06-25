namespace TaskVault.Contracts.Features.Email.Abstractions;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}