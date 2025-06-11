using TaskVault.Contracts.Features.FileStorage.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetTaskSubmissionDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid SubmittedById { get; set; }
    public DateTime SubmittedAt { get; set; }

    public IEnumerable<GetFileDto> TaskItemFiles { get; set; } = new List<GetFileDto>();
}