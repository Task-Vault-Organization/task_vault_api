using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetSharedFilesResponseDto : BaseApiResponse
{
    public IEnumerable<GetFileDto> Files { get; set; } = new List<GetFileDto>();

    public static GetSharedFilesResponseDto Create(string message, IEnumerable<GetFileDto> files)
    {
        return new GetSharedFilesResponseDto()
        {
            Message = message,
            Files = files
        };
    }
}