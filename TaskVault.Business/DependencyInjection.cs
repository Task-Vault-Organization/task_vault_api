using Microsoft.Extensions.DependencyInjection;
using TaskVault.Business.Features.FileStorage.Services;
using TaskVault.Business.Shared.Services;
using TaskVault.Contracts.Features.FileStorage;
using TaskVault.Contracts.Shared.Abstractions.Services;

namespace TaskVault.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection));
        services.AddTransient<IExceptionHandlingService, ExceptionHandlingService>();
        services.AddTransient<IFileStorageService, FileStorageService>();
        return services;
    }
}