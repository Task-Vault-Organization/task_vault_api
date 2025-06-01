using AutoMapper;
using Newtonsoft.Json;
using TaskVault.Business.Shared.Helpers;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.Business.Features.FileStorage.Services;

public class FileService : IFileService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IFileRepository _fileRepository;
    private readonly IFileTypeRepository _fileTypeRepository;
    private readonly IFileCategoryRepository _fileCategoryRepository;
    private readonly IMapper _mapper;
    private readonly IEntityValidator _entityValidator;
    private readonly IDirectoryEntryRepository _directoryEntryRepository;

    public FileService(
        IExceptionHandlingService exceptionHandlingService,
        IFileRepository fileRepository,
        IMapper mapper,
        IFileTypeRepository fileTypeRepository,
        IFileCategoryRepository fileCategoryRepository,
        IEntityValidator entityValidator, IDirectoryEntryRepository directoryEntryRepository)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _fileRepository = fileRepository;
        _mapper = mapper;
        _fileTypeRepository = fileTypeRepository;
        _fileCategoryRepository = fileCategoryRepository;
        _entityValidator = entityValidator;
        _directoryEntryRepository = directoryEntryRepository;
    }

    public async Task<GetFilesResponseDto> GetAllUserFilesAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var uploadedFiles = await _fileRepository.GetAllUploadedFilesAsync(user.Id);
            var sharedFiles = await _fileRepository.GetAllSharedFilesAsync(user.Id);

            var allFiles = uploadedFiles.Concat(sharedFiles)
                .DistinctBy(f => f.Id)
                .Select(f => _mapper.Map<GetFileDto>(f));

            return GetFilesResponseDto.Create("Successfully retrieved all accessible files", allFiles);
        }, "Error retrieving user files");
    }

    public async Task<GetFilesResponseDto> GetUploadedFilesByUserAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var uploadedFiles = await _fileRepository.GetAllUploadedFilesAsync(user.Id);
            var dtos = uploadedFiles.Select(f => _mapper.Map<GetFileDto>(f));
            return GetFilesResponseDto.Create("Successfully retrieved uploaded files", dtos);
        }, "Error retrieving uploaded files");
    }

    public async Task<GetFilesResponseDto> GetFilesSharedWithUserAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var sharedFiles = await _fileRepository.GetAllSharedFilesAsync(user.Id);
            var dtos = sharedFiles.Select(f => _mapper.Map<GetFileDto>(f));
            return GetFilesResponseDto.Create("Successfully retrieved shared files", dtos);
        }, "Error retrieving shared files");
    }


    public async Task<GetFilesResponseDto> GetAllDirectoryFilesAsync(string userEmail, Guid directoryId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var directory = await _entityValidator.GetFileOrThrowAsync(directoryId);

            var files = await _fileRepository.GetAllFilesInDirectoryForUserAsync(user.Id, directoryId);
            var result = files.Select(f => _mapper.Map<GetFileDto>(f));

            return GetFilesResponseDto.Create("Successfully retrieved directory files", result);
        }, "Error retrieving directory files");
    }
    
    public async Task<GetFilesResponseDto> GetUploadedFilesInDirectoryAsync(string userEmail, Guid directoryId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            
            var directory = await _entityValidator.GetFileOrThrowAsync(directoryId);

            var files = await _fileRepository.GetUploadedFilesInDirectoryAsync(user.Id, directoryId);
            var result = files.Select(f => _mapper.Map<GetFileDto>(f));

            return GetFilesResponseDto.Create("Successfully retrieved uploaded files", result);
        }, "Error retrieving uploaded files in directory");
    }
    
    public async Task<GetFilesResponseDto> GetSharedFilesInDirectoryAsync(string userEmail, Guid directoryId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            
            var directory = await _entityValidator.GetFileOrThrowAsync(directoryId);

            var files = await _fileRepository.GetSharedFilesInDirectoryAsync(user.Id, directoryId);
            var result = files.Select(f => _mapper.Map<GetFileDto>(f));

            return GetFilesResponseDto.Create("Successfully retrieved shared files", result);
        }, "Error retrieving shared files in directory");
    }

    public async Task<RenameFileResponseDto> RenameFileAsync(string userEmail, RenameFileDto renameFileDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(renameFileDto.FileId);

            var directoryEntry = (await _directoryEntryRepository.FindAsync(de =>
                de.UserId == user.Id && de.FileId == file.Id)).FirstOrDefault();

            if (directoryEntry is null)
                throw new UnauthorizedAccessException("User does not have access to rename this file");

            var directoryId = directoryEntry.DirectoryId;

            var baseName = Path.GetFileNameWithoutExtension(renameFileDto.Name);
            var extension = Path.GetExtension(renameFileDto.Name);
            var finalName = baseName + extension;

            var entriesInSameDirectory = await _directoryEntryRepository.FindAsync(de =>
                de.UserId == user.Id &&
                de.DirectoryId == directoryId &&
                de.FileId != file.Id);

            var fileIds = entriesInSameDirectory.Select(de => de.FileId).ToList();

            var existingFiles = await _fileRepository.FindAsync(f =>
                fileIds.Contains(f.Id) && f.UploaderId == user.Id && f.Name.StartsWith(baseName));

            var usedNames = new HashSet<string>(existingFiles.Select(f => f.Name));

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
                $"{user.Email} renamed this file from '{file.Name}' to '{finalName}'",
                _mapper.Map<GetUserDto>(user));

            FileHistoryHelper.AddFileHistoryLog(file, fileHistoryLog);
            file.Name = finalName;

            await _fileRepository.UpdateAsync(file, file.Id);

            return RenameFileResponseDto.Create("Successfully renamed file", _mapper.Map<GetFileDto>(file));
        }, "Error when renaming file");
    }

    public async Task<GetFileHistoryResponseDto> GetFileHistoryAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(fileId);
            _entityValidator.ValidateOwnership(file, user);

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
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var files = (await _fileRepository.GetAllSharedFilesAsync(user.Id))
                .Select(f => _mapper.Map<GetFileDto>(f));
            return GetSharedFilesResponseDto.Create("Successfully retrieved shared files", files);
        }, "Error when retrieving shared files");
    }

    public async Task<GetFileResponseDto> GetFileAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(fileId);
            _entityValidator.ValidateOwnership(file, user);
            return GetFileResponseDto.Create("Successfully retrieved file", _mapper.Map<GetFileDto>(file));
        }, "Error when retrieving file");
    }

    public async Task<BaseApiResponse> DeleteUploadedFileAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(fileId);
            _entityValidator.ValidateUploader(file, user);

            await _fileRepository.RemoveAsync(file);
            return BaseApiResponse.Create("Successfully deleted file");
        }, "Error when deleting file");
    }

    public async Task<GetFileTypeReponseDto> GetAllFileTypesAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            await _entityValidator.GetUserOrThrowAsync(userEmail);
            var fileTypes = (await _fileTypeRepository.GetAllAsync())
                .Select(ft => _mapper.Map<GetFileTypeDto>(ft));
            return GetFileTypeReponseDto.Create("Successfully retrieved file types", fileTypes);
        }, "Error when retrieving file types");
    }

    public async Task<GetFileCategoriesResponseDto> GetAllFileCategoriesAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            await _entityValidator.GetUserOrThrowAsync(userEmail);
            var fileCategories = (await _fileCategoryRepository.GetAllAsync())
                .Select(ft => _mapper.Map<GetFileCategoryDto>(ft));
            return GetFileCategoriesResponseDto.Create("Successfully retrieved file types", fileCategories);
        }, "Error when retrieving file categories");
    }
}