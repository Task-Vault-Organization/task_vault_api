using AutoMapper;
using TaskVault.Contracts.Features.Tasks.Dtos;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;

namespace TaskVault.Business.Shared.MapperProfiles;

public class UsersMapperProfiles : Profile
{
    public UsersMapperProfiles()
    {
        CreateMap<User, GetUserDto>();
        CreateMap<User, GetTaskSubmissionUserDto>()
            .ForMember(dest => dest.Approved, opt => opt.Ignore());
    }
}