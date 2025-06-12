using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetTaskSubmittedItemFileCommentDto
{
    public Guid Id { get; set; }
    public required Guid TaskSubmissionId { get; set; }
    public required Guid TaskSubmissionTaskItemFileId { get; set; }
    public required Guid FromUserId { get; set; }
    public required string CommentHtml { get; set; }
    
    public virtual GetUserDto? FromUser { get; set; }
}