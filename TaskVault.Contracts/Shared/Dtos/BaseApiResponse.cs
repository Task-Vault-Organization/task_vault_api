namespace TaskVault.Contracts.Shared.Dtos;

public class BaseApiResponse
{
    public required string Message { get; set; }
    public IEnumerable<string>? Warnings { get; set; } = [];

    public static BaseApiResponse Create(string message, IEnumerable<string>? warnings = null)
    {
        return new BaseApiResponse()
        {
            Message = message,
            Warnings = warnings
        };
    }
}