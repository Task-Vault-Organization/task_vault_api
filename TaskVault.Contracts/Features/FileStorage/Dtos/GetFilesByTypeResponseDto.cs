using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFilesByTypeResponseDto : BaseApiResponse
{
    public required IEnumerable<GetFileDto> Files { get; set; }

    public static GetFilesByTypeResponseDto Create(string message, IEnumerable<GetFileDto> files)
    {
        return new GetFilesByTypeResponseDto()
        {
            Message = message,
            Files = files
        };
    }
}