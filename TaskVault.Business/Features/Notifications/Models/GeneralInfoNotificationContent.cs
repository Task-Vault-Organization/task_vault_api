using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Business.Features.Notifications.Models;

public class GeneralInfoNotificationContent
{
    public required GetUserDto User { get; set; }
    public required string Message { get; set; }
    public string? TargetLink { get; set; }
}