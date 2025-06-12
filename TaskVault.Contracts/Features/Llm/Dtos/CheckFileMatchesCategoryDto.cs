namespace TaskVault.Contracts.Features.Llm.Dtos;

public class CheckFileMatchesCategoryDto
{
    public required Guid FileId { get; set; }
    public required int FileCategoryId { get; set; }
}