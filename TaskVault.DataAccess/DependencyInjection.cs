using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MsaCookingAppDevContext>((options) =>
        {
            options.UseSqlite(configuration.GetConnectionString("MsaCookingAppDevContext"));
            services.AddTransient<IRepository<UploadedFile>, Repository<UploadedFile>>();
        });
        
        return services;
    }
}