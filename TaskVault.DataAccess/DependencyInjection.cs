using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsaCookingApp.DataAccess.Context;
using MsaCookingApp.DataAccess.Entities;
using MsaCookingApp.DataAccess.Repositories;
using MsaCookingApp.DataAccess.Repositories.Abstractions;

namespace MsaCookingApp.DataAccess;

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