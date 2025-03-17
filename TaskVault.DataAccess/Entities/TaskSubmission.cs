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
}