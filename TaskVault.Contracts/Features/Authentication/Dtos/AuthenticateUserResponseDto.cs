using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Authentication.Dtos;

public class AuthenticateUserResponseDto : BaseApiResponse
{
    public required string JwtToken { get; set; }

    public static AuthenticateUserResponseDto Create(string message, string jwtToken)
    {
        return new AuthenticateUserResponseDto()
        {
            Message = message,
            JwtToken = jwtToken
        };
    }
}