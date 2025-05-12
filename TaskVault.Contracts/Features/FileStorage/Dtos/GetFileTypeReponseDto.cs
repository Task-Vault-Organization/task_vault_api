using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileTypeReponseDto : BaseApiResponse
{
    public required IEnumerable<GetFileTypeDto> FileTypes { get; set; }

    public static GetFileTypeReponseDto Create(string message, IEnumerable<GetFileTypeDto> fileTypes)
    {
        return new GetFileTypeReponseDto()
        {
            Message = message,
            FileTypes = fileTypes
        };
    }
}