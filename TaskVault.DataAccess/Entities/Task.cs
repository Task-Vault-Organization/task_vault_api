using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class Task
{
    [Key] public Guid Id { get; set; }

    [MaxLength(30)] public required string Title { get; set; }

    [MaxLength(200)] public required string Description { get; set; }

    public required Guid OwnerId { get; set; }

    public required DateTime CreatedAt { get; set; }

    public DateTime? DeadlineAt { get; set; }

    public required int StatusId { get; set; }

    public virtual User? Owner { get; set; }
    public virtual TaskStatus? Status { get; set; }

    public virtual IEnumerable<User>? Assignees { get; set; }

    public virtual IEnumerable<TaskSubmission>? TaskSubmissions { get; set; }
    
    public virtual ICollection<TaskItem>? TaskItems { get; set; }

    public static Task Create(Guid id, string title, string description, Guid ownerId)
    {
        return new Task
        {
            Id = id,
            Title = title,
            Description = description,
            OwnerId = ownerId,
            CreatedAt = DateTime.Now,
            StatusId = 1
        };
    }

}