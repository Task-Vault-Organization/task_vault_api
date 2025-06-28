using TaskVault.Contracts.Shared.Dtos;
using TaskStatus = TaskVault.DataAccess.Entities.TaskStatus;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetOwnedTaskDto
{
    public Guid Id { get; set; }
    
    public required string Title { get; set; }
    
    public required string Description { get; set; }
    
    public required Guid OwnerId { get; set; }
    
    public GetUserDto? Owner { get; set; }
    
    public required DateTime CreatedAt { get; set; }
    
    public DateTime? DeadlineAt { get; set; }
    
    public required int StatusId { get; set; }
    
    public TaskStatus? Status { get; set; }

    public required IEnumerable<GetTaskSubmissionUserDto> Assignees { get; set; }

    public required IEnumerable<GetTaskItemDto> TaskItems { get; set; } = new List<GetTaskItemDto>();
}