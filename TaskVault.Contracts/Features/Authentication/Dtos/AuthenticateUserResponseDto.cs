using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Authentication.Dtos;

public class AuthenticateUserResponseDto : BaseApiResponse
{
    public string? JwtToken { get; set; }

    public required bool IsEmailConfirmed { get; set; }

    public required Guid UserId { get; set; }
    public static AuthenticateUserResponseDto Create(string message, string jwtToken, bool isEmailConfirmed, Guid userId)
    {
        return new AuthenticateUserResponseDto
        {
            Message = message,
            JwtToken = jwtToken,
            IsEmailConfirmed = isEmailConfirmed,
            UserId = userId
        };
    }

}