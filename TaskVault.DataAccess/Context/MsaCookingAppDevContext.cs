using Microsoft.EntityFrameworkCore;
using TaskVault.DataAccess.Entities;

namespace TaskVault.DataAccess.Context;

public class MsaCookingAppDevContext : DbContext
{
    public DbSet<UploadedFile> UploadedFiles { get; set; }

    public MsaCookingAppDevContext(DbContextOptions<MsaCookingAppDevContext> options)
    : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}