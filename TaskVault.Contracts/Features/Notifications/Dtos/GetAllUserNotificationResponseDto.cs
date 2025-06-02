using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Notifications.Dtos;

public class GetAllUserNotificationResponseDto : BaseApiResponse
{
    public IEnumerable<GetNotificationDto> Notifications { get; set; } = [];

    public static GetAllUserNotificationResponseDto Create(string message, IEnumerable<GetNotificationDto> notifications)
    {
        return new GetAllUserNotificationResponseDto()
        {
            Message = message,
            Notifications = notifications
        };
    }
}