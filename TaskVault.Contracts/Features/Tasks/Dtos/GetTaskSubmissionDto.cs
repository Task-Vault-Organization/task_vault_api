using TaskVault.Contracts.Features.FileStorage.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetTaskSubmissionDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid SubmittedById { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? DissaproveComment { get; set; }

    public IEnumerable<GetTaskSubmissionFileDto> TaskItemFiles { get; set; } = new List<GetTaskSubmissionFileDto>();
}

public class GetTaskSubmissionFileDto : GetFileDto
{
    public Guid TaskItemId { get; set; }
    public bool? AiApproved { get; set; }
}