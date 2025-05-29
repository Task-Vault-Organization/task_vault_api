using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class RenameFileResponseDto : BaseApiResponse
{
    public required GetFileDto RenamedFile { get; set; }

    public static RenameFileResponseDto Create(string messasge, GetFileDto renamedFile)
    {
        return new RenameFileResponseDto()
        {
            Message = messasge,
            RenamedFile = renamedFile
        };
    }
}