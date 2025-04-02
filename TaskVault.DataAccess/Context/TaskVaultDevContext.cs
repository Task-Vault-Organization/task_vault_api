using Microsoft.EntityFrameworkCore;
using TaskVault.DataAccess.Entities;
using File = TaskVault.DataAccess.Entities.File;
using Task = TaskVault.DataAccess.Entities.Task;
using TaskStatus = TaskVault.DataAccess.Entities.TaskStatus;

namespace TaskVault.DataAccess.Context;

public class TaskVaultDevContext : DbContext
{
    public TaskVaultDevContext(DbContextOptions<TaskVaultDevContext> options)
        : base(options)
    {
    }

    public DbSet<File> Files { get; set; }
    public DbSet<FileCategory> FileCategories { get; set; }
    public DbSet<FileType> FileTypes { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<TaskStatus> TaskStatuses { get; set; }
    public DbSet<TaskSubmission> TaskSubmissions { get; set; }
    public DbSet<TaskSubmissionTaskItemFile> TaskSubmissionTaskItemFiles { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // File Relationships
        modelBuilder.Entity<File>()
            .HasOne(f => f.Uploader)
            .WithMany()
            .HasForeignKey(f => f.UploaderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<File>()
            .HasOne(f => f.FileType)
            .WithMany()
            .HasForeignKey(f => f.FileTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<File>()
            .HasMany<User>(f => f.Owners)
            .WithMany(u => u.Files)
            .UsingEntity<Dictionary<string, object>>( 
                "FileOwner", 
                j => j.HasOne<User>().WithMany().HasForeignKey("OwnerId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<File>().WithMany().HasForeignKey("FileId").OnDelete(DeleteBehavior.Cascade)
            );

        // Task Relationships
        modelBuilder.Entity<Task>()
            .HasOne(t => t.Owner)
            .WithMany()
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Task>()
            .HasMany(t => t.Assignees)
            .WithMany(u => u.Tasks)
            .UsingEntity<Dictionary<string, object>>(
                "TaskUsers",
                j => j.HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Task>()
                    .WithMany()
                    .HasForeignKey("TaskId")
                    .OnDelete(DeleteBehavior.Cascade)
            );
            
        
        modelBuilder.Entity<Task>()
            .HasOne(t => t.Status)
            .WithMany()
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // TaskItem Relationships
        modelBuilder.Entity<TaskItem>()
            .HasOne(ti => ti.FileType)
            .WithMany()
            .HasForeignKey(ti => ti.FileTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<TaskItem>()
            .HasOne<Task>(ti => ti.Task)
            .WithMany()
            .HasForeignKey(ti => ti.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<TaskItem>()
            .HasOne(ti => ti.FileCategory)
            .WithMany()
            .HasForeignKey(ti => ti.FileCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // TaskSubmission Relationships
        modelBuilder.Entity<TaskSubmission>()
            .HasOne(ts => ts.Task)
            .WithMany()
            .HasForeignKey(ts => ts.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<TaskSubmission>()
            .HasOne(ts => ts.SubmittedByUser)
            .WithMany()
            .HasForeignKey(ts => ts.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);

        // TaskSubmissionTaskItemFile Relationships
        modelBuilder.Entity<TaskSubmissionTaskItemFile>()
            .HasKey(tsif => new { tsif.TaskSubmissionId, tsif.TaskItemId, tsif.FileId });

        modelBuilder.Entity<TaskSubmissionTaskItemFile>()
            .HasOne<TaskSubmission>()
            .WithMany()
            .HasForeignKey(tsif => tsif.TaskSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<TaskSubmissionTaskItemFile>()
            .HasOne<TaskItem>()
            .WithMany()
            .HasForeignKey(tsif => tsif.TaskItemId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<TaskSubmissionTaskItemFile>()
            .HasOne<File>()
            .WithMany()
            .HasForeignKey(tsif => tsif.FileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}