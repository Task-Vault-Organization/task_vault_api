using Amazon.S3.Model;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFilesResponseDto : BaseApiResponse
{
    public IEnumerable<GetFileDto> Files { get; set; } = new List<GetFileDto>();

    public static GetFilesResponseDto Create(string message, IEnumerable<GetFileDto> files)
    {
        return new GetFilesResponseDto()
        {
            Message = message,
            Files = files
        };
    }
}