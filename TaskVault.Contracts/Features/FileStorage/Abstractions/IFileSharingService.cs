using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Abstractions;

public interface IFileSharingService
{
    Task<BaseApiResponse> CreateOrUpdateFileShareRequestAsync(string userEmail,
        CreateOrUpdateFileShareRequestDto createOrUpdateFileShareRequestDto);

    Task<BaseApiResponse> ResolveFileShareRequestAsync(string userEmail, ResolveFileShareRequestDto resolveFileShareRequestDto);
    Task<GetFileShareDataResponseDto> GetFileShareDataAsync(string userEmail, Guid fileId);
}