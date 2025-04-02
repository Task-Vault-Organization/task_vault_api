using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetTasksResponseDto : BaseApiResponse
{
    public required IEnumerable<GetTaskDto> Task { get; set; }

    public static GetTasksResponseDto Create(string message, IEnumerable<GetTaskDto> tasks)
    {
        return new GetTasksResponseDto()
        {
            Message = message,
            Task = tasks
        };
    }
}