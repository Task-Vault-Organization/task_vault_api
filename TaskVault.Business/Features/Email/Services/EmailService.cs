using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.Email.Abstractions;
using TaskVault.Contracts.Shared.Abstractions.Services;

namespace TaskVault.Business.Features.Email.Services;

public class EmailService : IEmailService
{
    private readonly IOptions<EmailOptions>  _options;
    private readonly IExceptionHandlingService _exceptionHandlingService;
    public EmailService(IOptions<EmailOptions> options, IExceptionHandlingService exceptionHandlingService)
    {
        _options = options;
        _exceptionHandlingService = exceptionHandlingService;
    }
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            string? fromEmail = _options.Value.SenderEmail;
            string? fromName = _options.Value.SenderName;
            string? apiKey = _options.Value.ApiKey;

            var sendGridClient = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(email);
            var plainTextContent = Regex.Replace(htmlMessage, "<[^>]*>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, subject,
                plainTextContent, htmlMessage);

            await sendGridClient.SendEmailAsync(msg);
        }, "Error when sending email");
    }
}