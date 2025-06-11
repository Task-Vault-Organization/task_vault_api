using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetAssignedTaskResponseDto : BaseApiResponse
{
    public required GetAssignedTaskDto Task { get; set; }

    public static GetAssignedTaskResponseDto Create(string message, GetAssignedTaskDto task)
    {
        return new GetAssignedTaskResponseDto()
        {
            Message = message,
            Task = task
        };
    }
}