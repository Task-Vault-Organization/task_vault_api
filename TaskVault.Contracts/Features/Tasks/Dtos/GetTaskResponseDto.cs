using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetTaskResponseDto : BaseApiResponse
{
    public required GetTaskDto Task { get; set; }

    public static GetTaskResponseDto Create(string message, GetTaskDto task)
    {
        return new GetTaskResponseDto()
        {
            Message = message,
            Task = task
        };
    }
}