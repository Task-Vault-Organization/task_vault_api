using AutoMapper;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.DataAccess.Entities;
using File = TaskVault.DataAccess.Entities.File;

namespace TaskVault.Business.Shared.MapperProfiles;

public class FilesMapperProfiles : Profile
{
    public FilesMapperProfiles()
    {
        CreateMap<File, GetFileDto>();
        CreateMap<FileType, GetFileTypeDto>();
        CreateMap<FileCategory, GetFileCategoryDto>();
    }
}