namespace TaskVault.Contracts.Shared.Dtos;

public class BaseApiResponse
{
    public required string Message { get; set; }

    public static BaseApiResponse Create(string message)
    {
        return new BaseApiResponse()
        {
            Message = message
        };
    }
}