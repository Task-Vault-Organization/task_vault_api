using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Llm.Dtos;

public class CheckFileMatchesCategoryResponseDto : BaseApiResponse
{
    public required double MatchPercentage { get; set; }

    public static CheckFileMatchesCategoryResponseDto Create(string message, double matchPercentage)
    {
        return new CheckFileMatchesCategoryResponseDto()
        {
            Message = message,
            MatchPercentage = matchPercentage
        };
    }
}