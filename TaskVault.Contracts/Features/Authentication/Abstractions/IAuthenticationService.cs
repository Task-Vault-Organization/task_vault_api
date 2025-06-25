using TaskVault.Contracts.Features.Authentication.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Authentication.Abstractions;

public interface IAuthenticationService
{
    Task<CreateUserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<AuthenticateUserResponseDto> AuthenticateUserAsync(AuthenticateUserDto authenticateUserDto);
    Task<AuthenticateUserResponseDto> AuthenticateUserGoogleAsync(AuthenticateUserGoogleDto authenticateUserDto);
    Task<GetUserResponseDto> GetUserAsync(string userEmail);

    Task<CheckIfUserHasEmailConfirmationRequestsResponse> CheckIfUserHasEmailConfirmationRequests(Guid userId);
    Task<BaseApiResponse> CreateEmailConfirmationRequestForUser(Guid userId);
    Task<BaseApiResponse> VerifyEmailConfirmationCodeAsync(Guid userId, string code);
}