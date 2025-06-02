using TaskVault.Contracts.Features.Notifications.Dtos;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskVault.Contracts.Features.Notifications.Abstractions;

public interface INotificationsService
{
    Task SendAndSaveNotificationAsync(Guid userId, Notification notification);
    Task<GetAllUserNotificationResponseDto> GetAllUserNotificationsAsync(string userEmail);
    Task<BaseApiResponse> MarkNotificationAsSeenAsync(string userEmail, Guid notificationId);
}