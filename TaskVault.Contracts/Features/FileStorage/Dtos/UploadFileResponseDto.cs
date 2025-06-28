using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class UploadFileResponseDto : BaseApiResponse
{
    public IEnumerable<GetFileDto>? UploadedFiles { get; set; }

    public static UploadFileResponseDto Create(string message, List<string> warnings, IEnumerable<GetFileDto> files)
    {
        return new UploadFileResponseDto()
        {
            Message = message,
            Warnings = warnings,
            UploadedFiles = files
        };
    }
}