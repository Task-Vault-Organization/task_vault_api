namespace TaskVault.DataAccess.Entities;

public class TaskSubmission
{
    public Guid Id { get; set; }

    public required Guid TaskId { get; set; }

    public required Guid SubmittedById { get; set; }

    public required DateTime SubmittedAt { get; set; }

    public required bool Approved { get; set; } = false;

    public virtual Task? Task { get; set; }

    public virtual User? SubmittedByUser { get; set; }

    public static TaskSubmission Create(Guid id, Guid taskId, Guid submittedById)
    {
        return new TaskSubmission
        {
            Id = id,
            TaskId = taskId,
            SubmittedById = submittedById,
            SubmittedAt = DateTime.Now,
            Approved = false
        };
    }
}