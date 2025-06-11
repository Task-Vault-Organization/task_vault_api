using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetOwnedTasksResponseDto : BaseApiResponse
{
    public IEnumerable<GetOwnedTaskDto> Tasks { get; set; } = new List<GetOwnedTaskDto>();

    public static GetOwnedTasksResponseDto Create(string message, IEnumerable<GetOwnedTaskDto> tasks)
    {
        return new GetOwnedTasksResponseDto()
        {
            Message = message,
            Tasks = tasks
        };
    }
}