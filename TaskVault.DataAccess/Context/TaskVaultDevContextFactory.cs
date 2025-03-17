using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TaskVault.DataAccess.Context
{
    public class TaskVaultDevContextFactory : IDesignTimeDbContextFactory<TaskVaultDevContext>
    {
        public TaskVaultDevContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TaskVaultDevContext>();
            optionsBuilder.UseSqlite(config.GetConnectionString("TaskVaultDevContext"));

            return new TaskVaultDevContext(optionsBuilder.Options);
        }
    }
}