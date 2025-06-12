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
    public DbSet<TaskSubmissionTaskItemFileComment> TaskSubmissionTaskItemFileComments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<CustomFileCategory> CustomFileCategories { get; set; }
    public DbSet<DirectoryEntry> DirectoryEntries { get; set; }
    public DbSet<FileShareRequest> FileShareRequests { get; set; }
    public DbSet<FileShareRequestStatus> FileShareRequestStatuses { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationType> NotificationTypes { get; set; }
    public DbSet<NotificationStatus> NotificationStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<File>()
            .HasMany(f => f.AsDirectoryEntries)
            .WithOne(de => de.Directory)
            .HasForeignKey(de => de.DirectoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<File>()
            .HasMany(f => f.AsFileEntries)
            .WithOne(de => de.File)
            .HasForeignKey(de => de.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.DirectoryEntries)
            .WithOne(de => de.User)
            .HasForeignKey(de => de.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomFileCategory>()
            .HasOne<User>(cfg => cfg.User)
            .WithMany()
            .HasForeignKey(cfg => cfg.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Task>()
            .HasOne(t => t.Owner)
            .WithMany()
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Task>()
            .HasMany(t => t.TaskSubmissions)
            .WithOne(ts => ts.Task)
            .HasForeignKey(ts => ts.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Task>()
            .HasMany(t => t.Assignees)
            .WithMany(u => u.Tasks)
            .UsingEntity<Dictionary<string, object>>(
                "TaskUsers",
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Task>().WithMany().HasForeignKey("TaskId").OnDelete(DeleteBehavior.Cascade)
            );

        modelBuilder.Entity<Task>()
            .HasOne(t => t.Status)
            .WithMany()
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasOne(ti => ti.FileType)
            .WithMany()
            .HasForeignKey(ti => ti.FileTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasOne(ti => ti.Task)
            .WithMany(t => t.TaskItems)
            .HasForeignKey(ti => ti.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasOne(ti => ti.FileCategory)
            .WithMany()
            .HasForeignKey(ti => ti.FileCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<DirectoryEntry>()
            .HasKey(de => new { de.UserId, de.DirectoryId, de.FileId });

        modelBuilder.Entity<DirectoryEntry>()
            .HasOne(de => de.User)
            .WithMany(u => u.DirectoryEntries)
            .HasForeignKey(de => de.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DirectoryEntry>()
            .HasOne(de => de.Directory)
            .WithMany(f => f.AsDirectoryEntries)
            .HasForeignKey(de => de.DirectoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<DirectoryEntry>()
            .HasOne(de => de.File)
            .WithMany(f => f.AsFileEntries)
            .HasForeignKey(de => de.FileId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<FileShareRequest>()
            .HasOne(fsr => fsr.From)
            .WithMany()
            .HasForeignKey(fsr => fsr.FromId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FileShareRequest>()
            .HasOne(fsr => fsr.To)
            .WithMany()
            .HasForeignKey(fsr => fsr.ToId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FileShareRequest>()
            .HasOne(fsr => fsr.File)
            .WithMany()
            .HasForeignKey(fsr => fsr.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FileShareRequest>()
            .HasOne(fsr => fsr.Status)
            .WithMany()
            .HasForeignKey(fsr => fsr.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.ToUser)
            .WithMany()
            .HasForeignKey(n => n.ToId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.NotificationType)
            .WithMany()
            .HasForeignKey(n => n.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.NotificationStatus)
            .WithMany()
            .HasForeignKey(n => n.NotificationStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskSubmissionTaskItemFileComment>()
            .HasOne(c => c.TaskSubmission)
            .WithMany()
            .HasForeignKey(c => c.TaskSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskSubmissionTaskItemFileComment>()
            .HasOne(c => c.TaskSubmissionTaskItemFile)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => new { c.TaskSubmissionId, c.TaskSubmissionTaskItemFileId, c.FromUserId })
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder.Entity<TaskSubmissionTaskItemFileComment>()
            .HasOne(c => c.FromUser)
            .WithMany()
            .HasForeignKey(c => c.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskSubmissionTaskItemFile>()
            .HasOne<File>(tf => tf.File)
            .WithMany()
            .HasForeignKey(tf => tf.FileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}