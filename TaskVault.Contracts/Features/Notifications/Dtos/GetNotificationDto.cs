using Firebase.Auth;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;

namespace TaskVault.Contracts.Features.Notifications.Dtos;

public class GetNotificationDto
{
    public Guid Id { get; set; }
    public required Guid ToId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string ContentJson { get; set; }
    public required int NotificationTypeId { get; set; }
    public required int NotificationStatusId { get; set; }

    public GetUserDto? ToUser { get; set; }
    public NotificationType? NotificationType { get; set; }
    public NotificationStatus? NotificationStatus { get; set; }
}