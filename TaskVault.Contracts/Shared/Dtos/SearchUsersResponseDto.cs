namespace TaskVault.Contracts.Shared.Dtos;

public class SearchUsersResponseDto : BaseApiResponse
{
    public required IEnumerable<GetUserDto> Users { get; set; }

    public static SearchUsersResponseDto Create(string message, IEnumerable<GetUserDto> users)
    {
        return new SearchUsersResponseDto()
        {
            Message = message,
            Users = users
        };
    }
}