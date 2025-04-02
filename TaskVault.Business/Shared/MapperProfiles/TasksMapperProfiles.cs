using AutoMapper;
using TaskVault.Contracts.Features.Tasks.Dtos;
using TaskVault.DataAccess.Entities;
using Task = TaskVault.DataAccess.Entities.Task;

namespace TaskVault.Business.Shared.MapperProfiles;

public class TasksMapperProfiles : Profile
{
    public TasksMapperProfiles()
    {
        CreateMap<TaskVault.DataAccess.Entities.Task, GetTaskDto>()
            .ForMember(dest => dest.Assignees, opt => opt.MapFrom(src => src.Assignees))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status)) // Map Status explicitly
            .ForMember(dest => dest.TaskItems, opt => opt.Ignore()); 
        
        CreateMap<TaskItem, GetTaskItemDto>();
    }
}
