using TaskVault.Contracts.Shared.Dtos;
using TaskStatus = TaskVault.DataAccess.Entities.TaskStatus;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetAssignedTaskDto
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

    public bool? Approved { get; set; }

    public required int NoComments { get; set; } = 0;
    public string? DissaprovedComment { get; set; }
    
    public required IEnumerable<GetAssignedTaskItemDto> TaskItems { get; set; } = new List<GetAssignedTaskItemDto>();
}