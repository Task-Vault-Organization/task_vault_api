using Microsoft.Extensions.DependencyInjection;
using TaskVault.Business.Features.Email.Services;
using TaskVault.Business.Features.FileStorage.Services;
using TaskVault.Business.Features.Llm.Services;
using TaskVault.Business.Features.Notifications.Services;
using TaskVault.Business.Features.Tasks.Services;
using TaskVault.Business.Shared.Services;
using TaskVault.Business.Shared.Validator;
using TaskVault.Contracts.Features.Email.Abstractions;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using TaskVault.Contracts.Features.Llm.Abstractions;
using TaskVault.Contracts.Features.Notifications.Abstractions;
using TaskVault.Contracts.Features.Tasks.Abstractions;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using AuthenticationService = TaskVault.Business.Features.Authentication.Services.AuthenticationService;
using IAuthenticationService = TaskVault.Contracts.Features.Authentication.Abstractions.IAuthenticationService;

namespace TaskVault.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection));
        services.AddTransient<IExceptionHandlingService, ExceptionHandlingService>();
        services.AddTransient<IFileStorageService, FileStorageService>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<ITaskService, TaskService>();
        services.AddTransient<IUsersService, UsersService>();
        services.AddScoped<IEntityValidator, EntityValidator>();
        services.AddScoped<IFileSharingService, FileSharingService>();
        services.AddTransient<INotificationsService, NotificationsService>();
        services.AddTransient<IFileHelpersService, FileHelpersService>();
        services.AddTransient<ILlmService, LlmService>();
        services.AddTransient<IEmailService, EmailService>();
        return services;
    }
}