using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Hubs;
using TaskVault.Contracts.Features.Notifications.Abstractions;
using TaskVault.Contracts.Features.Notifications.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace TaskVault.Business.Features.Notifications.Services;

public class NotificationsService : INotificationsService
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IEntityValidator _entityValidator;
    private readonly INotificationRepository _notificationsRepository;
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IMapper _mapper;

    public NotificationsService(IHubContext<NotificationHub> hub, IEntityValidator entityValidator, INotificationRepository notificationsRepository, IExceptionHandlingService exceptionHandlingService, IMapper mapper)
    {
        _hub = hub;
        _entityValidator = entityValidator;
        _notificationsRepository = notificationsRepository;
        _exceptionHandlingService = exceptionHandlingService;
        _mapper = mapper;
    }

    public async Task SendAndSaveNotificationAsync(Guid userId, Notification notification)
    {
        await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            var notificationJson = JsonConvert.SerializeObject(notification, settings);
            await SendToUserAsync(notification.ToId.ToString(), notificationJson);

            await _notificationsRepository.AddAsync(notification);
        }, "Error when sending and saving notification");
    }

    public async Task<GetAllUserNotificationResponseDto> GetAllUserNotificationsAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = await _entityValidator.GetUserOrThrowAsync(userEmail);

            var thresholdDate = DateTime.UtcNow.AddDays(-5);

            var userNotifications =
                (await _notificationsRepository.FindAsync(n =>
                    n.ToId == foundUser.Id && n.CreatedAt >= thresholdDate))
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => _mapper.Map<GetNotificationDto>(n));

            return GetAllUserNotificationResponseDto.Create("Successfully retrieved notifications", userNotifications);
        }, "Error when retrieving user notifications");
    }

    public async Task<BaseApiResponse> MarkNotificationAsSeenAsync(string userEmail, Guid notificationId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = await _entityValidator.GetUserOrThrowAsync(userEmail);

            var foundNotification = await _notificationsRepository.GetByIdAsync(notificationId);
            if (foundNotification == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "Notification not found");
            
            if (foundNotification.ToId != foundUser.Id)
                throw new ServiceException(StatusCodes.Status403Forbidden, "Forbidden to modify notification");

            foundNotification.NotificationStatusId = 2;
            await _notificationsRepository.UpdateAsync(foundNotification, foundNotification.Id);

            return BaseApiResponse.Create("Successfully updated notification");
        }, "Error when retrieving user notifications");
    }
    
    private async Task SendToUserAsync(string userId, string message)
    {
        var connections = NotificationHub.GetUserConnections(userId);
        foreach (var connId in connections)
        {
            await _hub.Clients.Client(connId).SendAsync("ReceiveNotification", message);
        }
    }
}