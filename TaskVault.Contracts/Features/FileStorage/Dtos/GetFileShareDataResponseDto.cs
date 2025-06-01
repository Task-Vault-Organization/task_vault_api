using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileShareDataResponseDto : BaseApiResponse
{
    public IEnumerable<GetFileShareDataUserItem> Items { get; set; } = [];

    public static GetFileShareDataResponseDto Create(string message, IEnumerable<GetFileShareDataUserItem> items)
    {
        return new GetFileShareDataResponseDto()
        {
            Message = message,
            Items = items
        };
    }
}