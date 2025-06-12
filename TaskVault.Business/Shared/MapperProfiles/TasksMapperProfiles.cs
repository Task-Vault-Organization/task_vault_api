using AutoMapper;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Features.Tasks.Dtos;
using TaskVault.DataAccess.Entities;
using Task = TaskVault.DataAccess.Entities.Task;

namespace TaskVault.Business.Shared.MapperProfiles;

public class TasksMapperProfiles : Profile
{
    public TasksMapperProfiles()
    {
        CreateMap<Task, GetTaskDto>()
            .ForMember(dest => dest.Assignees, opt => opt.MapFrom(src => src.Assignees))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.TaskItems, opt => opt.Ignore());

        CreateMap<TaskItem, GetTaskItemDto>();

        CreateMap<Task, GetOwnedTaskDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Assignees, opt => opt.Ignore()); // populated manually

        CreateMap<User, GetTaskSubmissionUserDto>()
            .ForMember(dest => dest.Approved, opt => opt.Ignore()); // handled manually

        CreateMap<Task, GetAssignedTaskDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Approved, opt => opt.Ignore()) // handled manually
            .ForMember(dest => dest.NoComments, opt => opt.Ignore()); // not implemented yet
        
        CreateMap<TaskSubmissionTaskItemFile, GetFileDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.File.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.File.Name))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.File.Size))
            .ForMember(dest => dest.UploaderId, opt => opt.MapFrom(src => src.File.UploaderId))
            .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => src.File.UploadedAt))
            .ForMember(dest => dest.FileTypeId, opt => opt.MapFrom(src => src.File.FileTypeId))
            .ForMember(dest => dest.IsDirectory, opt => opt.MapFrom(src => src.File.IsDirectory))
            .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.File.FileType))
            .ForMember(dest => dest.Uploader, opt => opt.MapFrom(src => src.File.Uploader))
            .ForMember(dest => dest.Owners, opt => opt.MapFrom(src => src.File.Owners));

        CreateMap<TaskSubmissionTaskItemFileComment, GetTaskSubmittedItemFileCommentDto>();
        CreateMap<TaskSubmission, GetTaskSubmissionDto>();
    }
}