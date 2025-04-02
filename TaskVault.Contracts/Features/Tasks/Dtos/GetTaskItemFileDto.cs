namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetTaskItemFileDto
{
    public required Guid TaskItemId { get; set; }

    public required Guid FileId { get; set; }
}