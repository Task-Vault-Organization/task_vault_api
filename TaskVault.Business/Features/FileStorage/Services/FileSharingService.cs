using AutoMapper;
using TaskVault.DataAccess.Entities;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Repositories.Abstractions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TaskVault.Business.Features.Notifications.Models;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Contracts.Features.FileStorage.Abstractions;
using TaskVault.Contracts.Features.Notifications.Abstractions;
using TaskVault.Contracts.Shared.Abstractions.Services;

namespace TaskVault.Business.Features.FileStorage.Services;

public class FileSharingService : IFileSharingService
{
    private readonly IEntityValidator _entityValidator;
    private readonly IFileShareRequestRepository _fileShareRequestRepository;
    private readonly IFileShareRequestStatusRepository _fileShareRequestStatusRepository;
    private readonly IDirectoryEntryRepository _directoryEntryRepository;
    private readonly IFileRepository _fileRepository;
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IMapper _mapper;
    private readonly INotificationsService _notificationsService;

    public FileSharingService(
        IEntityValidator entityValidator,
        IFileShareRequestRepository fileShareRequestRepository,
        IFileShareRequestStatusRepository fileShareRequestStatusRepository,
        IDirectoryEntryRepository directoryEntryRepository,
        IFileRepository fileRepository, IExceptionHandlingService exceptionHandlingService, IMapper mapper, INotificationsService notificationsService)
    {
        _entityValidator = entityValidator;
        _fileShareRequestRepository = fileShareRequestRepository;
        _fileShareRequestStatusRepository = fileShareRequestStatusRepository;
        _directoryEntryRepository = directoryEntryRepository;
        _fileRepository = fileRepository;
        _exceptionHandlingService = exceptionHandlingService;
        _mapper = mapper;
        _notificationsService = notificationsService;
    }

    public async Task<BaseApiResponse> CreateOrUpdateFileShareRequestAsync(string userEmail, CreateOrUpdateFileShareRequestDto dto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var fromUser = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var file = await _entityValidator.GetFileOrThrowAsync(dto.FileId);

            _entityValidator.ValidateOwnership(file, fromUser);

            var ownerIds = file.Owners?.Select(o => o.Id).ToHashSet() ?? new HashSet<Guid>();
            var toUserSet = dto.ToUsers.ToHashSet();
            var usersToRemove = ownerIds
                .Where(ownerId => ownerId != fromUser.Id && !toUserSet.Contains(ownerId))
                .ToList();

            foreach (var userId in usersToRemove)
            {
                file.Owners = file.Owners?.Where(o => o.Id != userId).ToList();
                await _fileRepository.UpdateAsync(file, file.Id);

                var entry = (await _directoryEntryRepository.FindAsync(de =>
                    de.FileId == file.Id && de.UserId == userId)).FirstOrDefault();
                if (entry != null)
                    await _directoryEntryRepository.RemoveAsync(entry);

                if (file.IsDirectory)
                {
                    var childEntries = await _directoryEntryRepository.FindAsync(de =>
                        de.DirectoryId == file.Id && de.UserId == userId);

                    foreach (var childEntry in childEntries)
                        await _directoryEntryRepository.RemoveAsync(childEntry);
                }
            }

            var existingRequests = await _fileShareRequestRepository.FindAsync(r =>
                r.FromId == fromUser.Id &&
                dto.ToUsers.Contains(r.ToId) &&
                r.FileId == file.Id &&
                r.StatusId == 1);

            var existingToUserIds = existingRequests.Select(r => r.ToId).ToHashSet();
            var newRequests = new List<FileShareRequest>();

            foreach (var toUserId in dto.ToUsers)
            {
                if (toUserId == fromUser.Id || existingToUserIds.Contains(toUserId) || ownerIds.Contains(toUserId))
                    continue;

                await _entityValidator.GetUserOrThrowAsync(toUserId);

                var request = FileShareRequest.Create(
                    fromId: fromUser.Id,
                    toId: toUserId,
                    fileId: file.Id,
                    statusId: 1);

                newRequests.Add(request);
            }

            if (newRequests.Count == 0 && usersToRemove.Count == 0)
                throw new ServiceException(StatusCodes.Status409Conflict, "All the specified users already have this file shared with them");

            if (newRequests.Count > 0)
                await _fileShareRequestRepository.AddRangeAsync(newRequests);
            
            foreach (var fileShareRequest in newRequests)
            {
                var notificationContent = AcceptFileShareNotificationContent.Create(_mapper.Map<GetUserDto>(fromUser),
                    _mapper.Map<GetFileDto>(file));
                var notificationJson = JsonConvert.SerializeObject(notificationContent);
                var notification = Notification.Create(Guid.NewGuid(), fileShareRequest.ToId, DateTime.Now, notificationJson, 1, 1);

                await _notificationsService.SendAndSaveNotificationAsync(fromUser.Id, notification);
            }

            return BaseApiResponse.Create("File share requests updated successfully");
        }, "Error when creating or updating file share request");
    }

    public async Task<BaseApiResponse> ResolveFileShareRequest(string userEmail, ResolveFileShareRequestDto resolveFileShareRequestDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var request = await _fileShareRequestRepository.GetByIdAsync(resolveFileShareRequestDto.FileShareRequestId);
            if (request == null)
                throw new ServiceException(StatusCodes.Status404NotFound, "File share request not found");

            if (request.ToId != user.Id)
                throw new ServiceException(StatusCodes.Status403Forbidden, "You are not authorized to resolve this request");

            if (request.StatusId != 1)
                throw new ServiceException(StatusCodes.Status409Conflict, "Only pending requests can be resolved");

            var status = await _fileShareRequestStatusRepository.GetByIdAsync(resolveFileShareRequestDto.ResponseStatusId);
            if (status == null)
                throw new ServiceException(StatusCodes.Status400BadRequest, "Invalid status");

            request.StatusId = resolveFileShareRequestDto.ResponseStatusId;
            await _fileShareRequestRepository.UpdateAsync(request, request.Id);

            if (resolveFileShareRequestDto.ResponseStatusId == 3)
            {
                var file = await _entityValidator.GetFileOrThrowAsync(request.FileId);

                if (file.Owners?.Any(o => o.Id == user.Id) != true)
                {
                    var owners = file.Owners?.ToList() ?? new List<User>();
                    owners.Add(user);
                    file.Owners = owners;
                    await _fileRepository.UpdateAsync(file, file.Id);
                }

                var entry = new DirectoryEntry
                {
                    UserId = user.Id,
                    DirectoryId = user.RootDirectoryId,
                    FileId = file.Id,
                    Index = 0
                };
                await _directoryEntryRepository.AddAsync(entry);

                if (file.IsDirectory)
                {
                    var childrenEntries = await _directoryEntryRepository.FindAsync(de =>
                        de.DirectoryId == file.Id && de.UserId == request.FromId);

                    foreach (var childEntry in childrenEntries)
                    {
                        var childFile = await _entityValidator.GetFileOrThrowAsync(childEntry.FileId);

                        if (childFile.IsDirectory)
                            continue;

                        if (childFile.Owners?.Any(o => o.Id == user.Id) != true)
                        {
                            var childOwners = childFile.Owners?.ToList() ?? new List<User>();
                            childOwners.Add(user);
                            childFile.Owners = childOwners;
                            await _fileRepository.UpdateAsync(childFile, childFile.Id);
                        }

                        var childDirEntry = new DirectoryEntry
                        {
                            UserId = user.Id,
                            DirectoryId = file.Id,
                            FileId = childFile.Id,
                            Index = 0
                        };
                        await _directoryEntryRepository.AddAsync(childDirEntry);
                    }
                }
            }

            return BaseApiResponse.Create("File share request resolved successfully");
        }, "Error when resolving file share request");
    }

    public async Task<GetFileShareDataResponseDto> GetFileShareDataAsync(string userEmail, Guid fileId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var foundFile = await _entityValidator.GetFileOrThrowAsync(fileId);

            if (user.Id != foundFile.UploaderId)
                throw new ServiceException(StatusCodes.Status403Forbidden, "Forbidden to access file");

            var foundSharingRequests = await _fileShareRequestRepository
                .FindAsync((fsr) => fsr.FileId == foundFile.Id && fsr.FromId == user.Id);

            if (foundFile.Owners != null)
            {
                var fileShareDataItems = foundFile.Owners.Where((u) => u.Id != user.Id)
                    .Select((u) => GetFileShareDataUserItem.Create(u.Id, u.Email, null))
                    .Concat(foundSharingRequests.Select((fsr) =>
                        GetFileShareDataUserItem.Create(fsr.To!.Id, fsr.To.Email, fsr.Status)));

                return GetFileShareDataResponseDto.Create("Successfully retrieved file share data", fileShareDataItems);
            }
            
            return GetFileShareDataResponseDto.Create("Successfully retrieved file share data", []);
        }, "Error when retrieving file share data");
    }
}