namespace TaskVault.Contracts.Shared.Dtos;

public class BaseApiFileResponse
{
    public MemoryStream FileMemoryStream { get; set; } = null!;
    public string? ContentType { get; set; }

    public static BaseApiFileResponse Create(MemoryStream fileMemoryStream, string contentType = "")
    {
        return new BaseApiFileResponse()
        {
            FileMemoryStream = fileMemoryStream,
            ContentType = contentType
        };
    }
}