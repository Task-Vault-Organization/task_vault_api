using System.ComponentModel.DataAnnotations;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class CreateTaskDto
{
    [MaxLength(30)]
    public required string Title { get; set; }
    
    [MaxLength(200)]
    public required string Description { get; set; }
    
    public DateTime? DeadlineAt { get; set; }

    public required IEnumerable<CreateTaskItemDto> TaskItems { get; set; }

    public IEnumerable<Guid>? AssigneesIds { get; set; }
}