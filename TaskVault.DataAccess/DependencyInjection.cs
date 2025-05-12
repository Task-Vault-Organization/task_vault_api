using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskVault.DataAccess.Context;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaskVaultDevContext>((options) =>
        {
            options.UseSqlite(configuration.GetConnectionString("TaskVaultDevContext"))
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
        });
        services.AddTransient<IRepository<User>, Repository<User>>();
        services.AddTransient<IRepository<File>, FileRepository>();
        services.AddTransient<IRepository<FileType>, Repository<FileType>>();
        services.AddTransient<IFileRepository, FileRepository>();
        services.AddTransient<ITaskItemRepository, TaskItemRepository>();
        services.AddTransient<ITasksRepository, TasksRepository>();
        services.AddTransient<ITaskSubmissionRepository, TaskSubmissionRepository>();
        services.AddTransient<ITaskSubmissionTaskItemFileRepository, TaskSubmissionTaskItemFileRepository>();
        services.AddTransient<ICustomFileCategoryRepository, CustomFileCategoryRepository>();
        services.AddTransient<IFileTypeRepository, FileTypeRepository>();
        services.AddTransient<IFileCategoryRepository, FileCategoryRepository>();
        
        return services;
    }
}