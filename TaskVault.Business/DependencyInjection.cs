using Microsoft.Extensions.DependencyInjection;
using MsaCookingApp.Business.Features;
using MsaCookingApp.Business.Shared.Services;
using MsaCookingApp.Contracts.Features;
using MsaCookingApp.Contracts.Shared.Abstractions.Services;

namespace MsaCookingApp.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection));
        services.AddTransient<IExceptionHandlingService, ExceptionHandlingService>();
        services.AddTransient<IUploadFileService, UploadFileService>();
        return services;
    }
}