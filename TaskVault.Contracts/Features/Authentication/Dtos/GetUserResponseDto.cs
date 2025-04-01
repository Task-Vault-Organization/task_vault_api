using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Authentication.Dtos;

public class GetUserResponseDto : BaseApiResponse
{
    public required GetUserDto User { get; set; }

    public static GetUserResponseDto Create(string messgae, GetUserDto getUser)
    {
        return new GetUserResponseDto()
        {
            Message = messgae,
            User = getUser
        };
    }
}