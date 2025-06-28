namespace TaskVault.Contracts.Features.Llm.Dtos;

public class CategorizeFolderResponseDto
{
    public required string Message { get; set; }
    public required Dictionary<string, List<Guid>> CategorizedFileIds { get; set; }

    public static CategorizeFolderResponseDto Create(string message, Dictionary<string, List<Guid>> map)
    {
        return new CategorizeFolderResponseDto { Message = message, CategorizedFileIds = map };
    }
}