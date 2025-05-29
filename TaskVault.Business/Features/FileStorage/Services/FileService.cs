using AutoMapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Helpers;
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
    
    public async Task<GetUploadedFilesResponseDto> GetAllUploadedDirectoryFilesAsync(string userEmail, Guid? directoryId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var files = (await _fileRepository.FindAsync((f) =>
                    f.UploaderId == foundUser.Id && f.DirectoryId == directoryId))
                .OrderBy((f) => f.Index)
                .Select(f => _mapper.Map<GetFileDto>(f));

            return GetUploadedFilesResponseDto.Create("Successfully retrieved uploaded files", files);
        }, "Error when retrieving uploaded files");
    }

    public async Task<RenameFileResponseDto> RenameFileAsync(string userEmail, RenameFileDto renameFileDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");

            var foundFile = await _fileRepository.GetFileByIdAsync(renameFileDto.FileId);
            if (foundFile == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "File not found");

            var baseName = Path.GetFileNameWithoutExtension(renameFileDto.Name);
            var extension = Path.GetExtension(renameFileDto.Name);

            var existingFiles = await _fileRepository.FindAsync(f =>
                f.DirectoryId == foundFile.DirectoryId &&
                f.Id != foundFile.Id &&
                f.UploaderId == foundUser.Id &&
                f.Name.StartsWith(baseName));

            var usedNames = new HashSet<string>(existingFiles.Select(f => f.Name));

            var finalName = baseName + extension;
            if (usedNames.Contains(finalName))
            {
                int counter = 1;
                while (usedNames.Contains($"{baseName}({counter}){extension}"))
                {
                    counter++;
                }
                finalName = $"{baseName}({counter}){extension}";
            }

            var fileHistoryLog = FileHistoryLog.Create(
                $"{foundUser.Email} renamed this file from '{foundFile.Name}' to '{finalName}'",
                _mapper.Map<GetUserDto>(foundUser));

            FileHistoryHelper.AddFileHistoryLog(foundFile, fileHistoryLog);

            foundFile.Name = finalName;
            
            await _fileRepository.UpdateAsync(foundFile, foundFile.Id);

            return RenameFileResponseDto.Create("Successfully renamed file", _mapper.Map<GetFileDto>(foundFile));
        }, "Error when renaming file");
    }

    public async Task<GetFileHistoryResponseDto> GetFileHistoryAsync(string userEmail, Guid fileId)
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

            if (file.HistoryJson != null)
            {
                var fileHistoryLogs = JsonConvert.DeserializeObject<List<FileHistoryLog>>(file.HistoryJson);
                if (fileHistoryLogs != null)
                    return GetFileHistoryResponseDto.Create("Successfully retrieved file history", fileHistoryLogs);
            }

            return GetFileHistoryResponseDto.Create("The file has no history", []);
        }, "Error when retrieving file history");
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