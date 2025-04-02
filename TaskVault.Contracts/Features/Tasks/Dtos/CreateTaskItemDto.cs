using System.ComponentModel.DataAnnotations;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class CreateTaskItemDto
{
    [MaxLength(20)]
    public required string Title { get; set; }
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public required int FileTypeId { get; set; }

    public required int  FileCategoryId { get; set; }
}