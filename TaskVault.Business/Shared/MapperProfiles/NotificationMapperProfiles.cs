using AutoMapper;
using TaskVault.Contracts.Features.Notifications.Dtos;
using TaskVault.DataAccess.Entities;

namespace TaskVault.Business.Shared.MapperProfiles;

public class NotificationMapperProfiles : Profile
{
    public NotificationMapperProfiles()
    {
        CreateMap<Notification, GetNotificationDto>();
    }
}