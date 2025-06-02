using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Business.Features.Notifications.Models;

public class AcceptFileShareNotificationContent
{
    public required GetUserDto User { get; set; }
    public required GetFileDto File { get; set; }

    public static AcceptFileShareNotificationContent Create(GetUserDto user, GetFileDto file)
    {
        return new AcceptFileShareNotificationContent()
        {
            User = user,
            File = file
        };
    }
}