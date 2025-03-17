namespace TaskVault.DataAccess.Entities;

public class TaskSubmissionTaskItemFile
{
    public required Guid TaskSubmissionId { get; set; }

    public required Guid TaskItemId { get; set; }

    public required Guid FileId { get; set; }
}