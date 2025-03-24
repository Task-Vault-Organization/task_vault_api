using AutoMapper;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.Business.Shared.MapperProfiles;

public class FilesMapperProfiles : Profile
{
    public FilesMapperProfiles()
    {
        CreateMap<File, GetFileDto>();
    }
}