using System.ComponentModel.DataAnnotations;
using TaskVault.DataAccess.Entities;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetTaskItemDto
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public required Guid TaskId { get; set; }

    public required int FileTypeId { get; set; }

    public required int  FileCategoryId { get; set; }

    public virtual FileType? FileType { get; set; }

    public virtual FileCategory? FileCategory { get; set; }   
}