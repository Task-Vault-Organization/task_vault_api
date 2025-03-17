using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskVault.DataAccess.Context;

namespace TaskVault.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaskVaultDevContext>((options) =>
        {
            options.UseSqlite(configuration.GetConnectionString("TaskVaultDevContext"));
        });
        
        return services;
    }
}