namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class RenameFileDto
{
    public required Guid FileId { get; set; }
    public required string Name { get; set; }
}