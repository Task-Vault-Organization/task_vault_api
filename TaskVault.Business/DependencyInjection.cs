using Microsoft.Extensions.DependencyInjection;
using TaskVault.Business.Features.FileClassifierTrainer.Services;
using TaskVault.Business.Features.FileStorage.Services;
using TaskVault.Business.Features.Tasks.Services;
using TaskVault.Business.Shared.Services;
using TaskVault.Contracts.Features.FileClassifierTrainer.Abstractions;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using TaskVault.Contracts.Features.Tasks.Abstractions;
using TaskVault.Contracts.Shared.Abstractions.Services;
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
        services.AddTransient<IFileClassifierTrainer, FileClassifierTrainer>();
        return services;
    }
}