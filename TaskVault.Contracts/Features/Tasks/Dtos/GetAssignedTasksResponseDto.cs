using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetAssignedTasksResponseDto : BaseApiResponse
{
    public required IEnumerable<GetAssignedTaskDto> Tasks { get; set; } = new List<GetAssignedTaskDto>();

    public static GetAssignedTasksResponseDto Create(string message, IEnumerable<GetAssignedTaskDto> tasks)
    {
        return new GetAssignedTasksResponseDto()
        {
            Message = message,
            Tasks = tasks
        };
    }
}