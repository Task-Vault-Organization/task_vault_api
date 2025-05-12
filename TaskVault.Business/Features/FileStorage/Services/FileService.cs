using AutoMapper;
using Microsoft.AspNetCore.Http;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.Business.Features.FileStorage.Services;

public class FileService : IFileService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IRepository<User> _userRepository;
    private readonly IFileRepository _fileRepository;
    private readonly IFileTypeRepository _fileTypeRepository;
    private readonly IFileCategoryRepository _fileCategoryRepository;
    private readonly IMapper _mapper;

    public FileService(IExceptionHandlingService exceptionHandlingService, IRepository<User> userRepository, IFileRepository fileRepository, IMapper mapper, IFileTypeRepository fileTypeRepository, IFileCategoryRepository fileCategoryRepository)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _userRepository = userRepository;
        _fileRepository = fileRepository;
        _mapper = mapper;
        _fileTypeRepository = fileTypeRepository;
        _fileCategoryRepository = fileCategoryRepository;
    }

    public async Task<GetUploadedFilesResponseDto> GetAllUploadedFilesAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var files = (await _fileRepository.GetAllUploadedFilesAsync(foundUser.Id))
                .Select(f => _mapper.Map<GetFileDto>(f));

            return GetUploadedFilesResponseDto.Create("Successfully retrieved uploaded files", files);
        }, "Error when retrieving uploaded files");
    }

    public async Task<GetSharedFilesResponseDto> GetAllSharedFilesAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var files = (await _fileRepository.GetAllSharedFilesAsync(foundUser.Id))
                .Select(f => _mapper.Map<GetFileDto>(f));

            return GetSharedFilesResponseDto.Create("Successfully retrieved shared files", files);
        }, "Error when retrieving shared files");
    }

    public async Task<GetFileResponseDto> GetFileAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var file = await _fileRepository.GetFileByIdAsync(fileId);

            if (file == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "File not found");
            }
            
            if (file.Owners?.FirstOrDefault(u => u.Id == foundUser.Id) == null)
            {
                throw new ServiceException(StatusCodes.Status403Forbidden, "Forbidden access to file");
            }

            return GetFileResponseDto.Create("Successfully retrieved file", _mapper.Map<GetFileDto>(file));
        }, "Error when retrieving file");
    }

    public async Task<BaseApiResponse> DeleteUploadedFileAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var file = await _fileRepository.GetByIdAsync(fileId);

            if (file == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "File not found");
            }
            
            if (file.UploaderId != foundUser.Id)
            {
                throw new ServiceException(StatusCodes.Status403Forbidden, "You can only delete files you have uploaded");
            }

            await _fileRepository.RemoveAsync(file);

            return BaseApiResponse.Create("Successfully deleted file");
        }, "Error when deleting file");
    }

    public async Task<GetFileTypeReponseDto> GetAllFileTypesAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var fileTypes = (await _fileTypeRepository.GetAllAsync())
                .Select((ft) => _mapper.Map<GetFileTypeDto>(ft));
            return GetFileTypeReponseDto.Create("Successfully retrieved file types", fileTypes);
        }, "Error when retrieving file types");
    }

    public async Task<GetFileCategoriesResponseDto> GetAllFileCategoriesAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var fileCategories = (await _fileCategoryRepository.GetAllAsync())
                .Select((ft) => _mapper.Map<GetFileCategoryDto>(ft));
            return GetFileCategoriesResponseDto.Create("Successfully retrieved file types", fileCategories);
        }, "Error when retrieving file categories");
    }
}