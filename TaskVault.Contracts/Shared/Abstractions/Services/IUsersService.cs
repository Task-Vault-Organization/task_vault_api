using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Shared.Abstractions.Services;

public interface IUsersService
{
    Task<SearchUsersResponseDto> SearchUsersAsync(string userEmail, string searchField);
}