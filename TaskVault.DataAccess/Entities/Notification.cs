using System.ComponentModel.DataAnnotations;

namespace TaskVault.DataAccess.Entities;

public class Notification
{
    [Key]
    public Guid Id { get; set; }
    public required Guid ToId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string ContentJson { get; set; }
    public required int NotificationTypeId { get; set; }
    public required int NotificationStatusId { get; set; }

    public virtual User? ToUser { get; set; }
    public virtual NotificationType? NotificationType { get; set; }
    public virtual NotificationStatus? NotificationStatus { get; set; }

    public static Notification Create(Guid id, Guid toId, DateTime createdAt, string contentJson,
        int notificationTypeId, int notificationStatusId)
    {
        return new Notification()
        {
            Id = id,
            ToId = toId,
            CreatedAt = createdAt,
            ContentJson = contentJson,
            NotificationTypeId = notificationTypeId,
            NotificationStatusId = notificationStatusId
        };
    }
}