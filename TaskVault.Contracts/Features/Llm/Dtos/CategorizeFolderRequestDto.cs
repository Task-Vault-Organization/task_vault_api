namespace TaskVault.Contracts.Features.Llm.Dtos;

public class CategorizeFolderRequestDto
{
    public required Guid FolderId { get; set; }
    public required List<string> Categories { get; set; }
}