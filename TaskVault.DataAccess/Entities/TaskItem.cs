using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class TaskItem
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(20)]
    public required string Title { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    public required Guid TaskId { get; set; }

    public required int FileTypeId { get; set; }

    public required int  FileCategoryId { get; set; }

    public virtual FileType? FileType { get; set; }

    public virtual FileCategory? FileCategory { get; set; }
    
    public virtual Task? Task { get; set; }

    public static TaskItem Create(string title, int fileTypeId, int fileCategoryId, Guid taskId)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            FileTypeId = fileTypeId,
            FileCategoryId = fileCategoryId,
            TaskId = taskId
        };
    }
}