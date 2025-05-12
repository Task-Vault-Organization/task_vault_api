using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetFileCategoriesResponseDto : BaseApiResponse
{
    public required IEnumerable<GetFileCategoryDto> FileCategories { get; set; }

    public static GetFileCategoriesResponseDto Create(string message, IEnumerable<GetFileCategoryDto> fileCategories)
    {
        return new GetFileCategoriesResponseDto()
        {
            Message = message,
            FileCategories = fileCategories
        };
    }
}