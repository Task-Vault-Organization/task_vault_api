using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetOwnedTaskResponseDto : BaseApiResponse
{
    public required GetOwnedTaskDto Task { get; set; }

    public static GetOwnedTaskResponseDto Create(string message, GetOwnedTaskDto task)
    {
        return new GetOwnedTaskResponseDto()
        {
            Message = message,
            Task = task
        };
    }
}