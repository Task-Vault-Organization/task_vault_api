using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.Notifications.Abstractions;

namespace TaskVault.Api.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationsService _notificationsService;

    public NotificationController(INotificationsService notificationsService)
    {
        _notificationsService = notificationsService;
    }
    
    [HttpGet("user")]
    public async Task<IActionResult> GetAllUserNotificationsAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _notificationsService.GetAllUserNotificationsAsync(userEmail));
    }

    [HttpPatch("{notificationId}/seen")]
    public async Task<IActionResult> MarkNotificationAsSeenAsync(Guid notificationId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _notificationsService.MarkNotificationAsSeenAsync(userEmail, notificationId));
    }
}