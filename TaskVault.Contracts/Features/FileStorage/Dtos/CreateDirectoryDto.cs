namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class CreateDirectoryDto
{
    public required string DirectoryName { get; set; }
    public Guid ParentDirectoryId { get; set; }
}