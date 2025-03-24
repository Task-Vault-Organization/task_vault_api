using System.Security.Claims;

namespace TaskVault.Api.Helpers;

public class AuthorizationHelper
{
    public static string GetUserEmailFromClaims(ClaimsPrincipal user)
    {
        return user.Claims.ElementAt(0).Value;
    }
}