using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;

namespace TaskVault.Contracts.Features.FileStorage.Dtos;

public class GetAllCustomFileCategoriesResponseDto : BaseApiResponse
{
    public required IEnumerable<CustomFileCategory> CustomFileCategories { get; set; } = new List<CustomFileCategory>();

    public static GetAllCustomFileCategoriesResponseDto Create(string message, IEnumerable<CustomFileCategory> customFileCategories)
    {
        return new GetAllCustomFileCategoriesResponseDto()
        {
            Message = message,
            CustomFileCategories = customFileCategories
        };
    }
}