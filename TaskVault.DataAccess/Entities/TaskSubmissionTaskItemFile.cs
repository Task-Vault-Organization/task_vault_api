namespace TaskVault.DataAccess.Entities;

public class TaskSubmissionTaskItemFile
{
    public required Guid TaskSubmissionId { get; set; }

    public required Guid TaskItemId { get; set; }

    public required Guid FileId { get; set; }

    public virtual IEnumerable<TaskSubmissionTaskItemFileComment>? Comments { get; set; }

    public virtual File? File { get; set; }

    public static TaskSubmissionTaskItemFile Create(Guid taskSubmissionId, Guid taskItemId, Guid fileId)
    {
        return new TaskSubmissionTaskItemFile
        {
            TaskSubmissionId = taskSubmissionId,
            TaskItemId = taskItemId,
            FileId = fileId
        };
    }
}