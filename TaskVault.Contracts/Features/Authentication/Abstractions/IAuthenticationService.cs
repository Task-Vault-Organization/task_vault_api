using TaskVault.Contracts.Features.Authentication.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Authentication.Abstractions;

public interface IAuthenticationService
{
    Task<BaseApiResponse> CreateUserAsync(CreateUserDto createUserDto);
    Task<AuthenticateUserResponseDto> AuthenticateUserAsync(AuthenticateUserDto authenticateUserDto);
    Task<AuthenticateUserResponseDto> AuthenticateUserGoogleAsync(AuthenticateUserGoogleDto authenticateUserDto);
    Task<GetUserResponseDto> GetUserAsync(string userEmail);
}