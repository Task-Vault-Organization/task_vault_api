using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Authentication.Dtos;

public class CreateUserResponseDto : BaseApiResponse
{
    public required Guid UserId { get; set; }

    public static CreateUserResponseDto Create(string message, Guid userId)
    {
        return new CreateUserResponseDto()
        {
            Message = message,
            UserId = userId
        };
    }
}