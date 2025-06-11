using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class TaskSubmissionTaskItemFileComment
{
    public Guid Id { get; set; }
    public required Guid TaskSubmissionId { get; set; }
    public required Guid TaskSubmissionTaskItemFileId { get; set; }
    public required Guid FromUserId { get; set; }
    
    [MaxLength(1000)]
    public required string CommentHtml { get; set; }

    public virtual TaskSubmission? TaskSubmission { get; set; }
    public virtual TaskSubmissionTaskItemFile? TaskSubmissionTaskItemFile { get; set; }
    public virtual User? FromUser { get; set; }
}