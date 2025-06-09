namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class MoveFileToDirectoryDto
{
    public required Guid FileId { get; set; }
    public required Guid NewDirectoryId { get; set; }
}