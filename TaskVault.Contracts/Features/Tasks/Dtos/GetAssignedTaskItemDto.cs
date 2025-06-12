using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.DataAccess.Entities;

namespace TaskVault.Contracts.Features.Tasks.Dtos;

public class GetAssignedTaskItemDto
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public required Guid TaskId { get; set; }

    public required int FileTypeId { get; set; }

    public required int  FileCategoryId { get; set; }

    public FileType? FileType { get; set; }

    public FileCategory? FileCategory { get; set; }

    public GetFileDto? SubmittedFile { get; set; }

    public required IEnumerable<GetTaskSubmittedItemFileCommentDto> Comments { get; set; } = [];
}