using Amazon.S3.Model;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetUploadedFilesResponseDto : BaseApiResponse
{
    public IEnumerable<GetFileDto> Files { get; set; } = new List<GetFileDto>();

    public static GetUploadedFilesResponseDto Create(string message, IEnumerable<GetFileDto> files)
    {
        return new GetUploadedFilesResponseDto()
        {
            Message = message,
            Files = files
        };
    }
}