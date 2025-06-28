namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileCategoryDto
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public bool? Legacy { get; set; }
}