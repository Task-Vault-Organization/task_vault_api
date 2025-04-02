namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class CreateTaskSubmissionDto
{
    public required Guid TaskId { get; set; }

    public required IEnumerable<GetTaskItemFileDto> TaskItemFiles { get; set; }
}