using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileResponseDto : BaseApiResponse
{
    public required GetFileDto File { get; set; }

    public static GetFileResponseDto Create(string message, GetFileDto file)
    {
        return new GetFileResponseDto()
        {
            Message = message,
            File = file
        };
    }
}