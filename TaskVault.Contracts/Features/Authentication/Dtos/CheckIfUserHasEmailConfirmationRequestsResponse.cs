using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Authentication.Dtos;

public class CheckIfUserHasEmailConfirmationRequestsResponse : BaseApiResponse
{
    public bool EmailConfirmationRequestsExist { get; set; }

    public static CheckIfUserHasEmailConfirmationRequestsResponse Create(string message,
        bool emailConfirmationRequestsExist)
    {
        return new CheckIfUserHasEmailConfirmationRequestsResponse()
        {
            Message = message,
            EmailConfirmationRequestsExist = emailConfirmationRequestsExist
        };
    }
}