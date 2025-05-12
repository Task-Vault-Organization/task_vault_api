namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileTypeDto
{
    public int Id { get; set; }

    public required string Extension { get; set; }

    public required string Name { get; set; }
}