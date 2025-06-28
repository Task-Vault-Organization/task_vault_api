using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Business.Shared.Options;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;
using Task = System.Threading.Tasks.Task;

namespace TaskVault.Business.Shared.Validator;

public class EntityValidator : IEntityValidator
{
    private readonly IRepository<User> _userRepository;
    private readonly IFileRepository _fileRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly FileUploadOptions _fileUploadOptions;
    private readonly ITaskItemRepository _taskItemRepository;

    public EntityValidator(
        IRepository<User> userRepository,
        IFileRepository fileRepository,
        ITasksRepository tasksRepository, IOptions<FileUploadOptions> fileUploadOptions, ITaskItemRepository taskItemRepository)
    {
        _userRepository = userRepository;
        _fileRepository = fileRepository;
        _tasksRepository = tasksRepository;
        _taskItemRepository = taskItemRepository;
        _fileUploadOptions = fileUploadOptions.Value;
    }

    public async Task<User> GetUserOrThrowAsync(string email)
    {
        var user = (await _userRepository.FindAsync(u => u.Email == email)).FirstOrDefault();
        if (user == null) throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
        return user;
    }
    
    public async Task<TaskItem> GetTaskItemOrThrowAsync(Guid taskItemId)
    {
        var taskItem = await _taskItemRepository.GetByIdAsync(taskItemId);
        if (taskItem == null)
            throw new ServiceException(StatusCodes.Status404NotFound, "Task item not found");

        return taskItem;
    }

    public async Task<User> GetUserOrThrowAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
        return user;
    }

    public async Task EnsureUserDoesNotExistAsync(string email)
    {
        var user = (await _userRepository.FindAsync(u => u.Email == email)).FirstOrDefault();
        if (user != null) throw new ServiceException(StatusCodes.Status409Conflict, "Email already taken");
    }

    public async Task<User> EnsureUserExistsAsync(string email)
    {
        var user = (await _userRepository.FindAsync(u => u.Email == email)).FirstOrDefault();
        if (user == null) throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
        return user;
    }

    public Task ValidatePasswordAsync(string rawPassword, User user)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, rawPassword);
        if (result != PasswordVerificationResult.Success)
            throw new ServiceException(StatusCodes.Status401Unauthorized, "Wrong credentials");
        return Task.CompletedTask;
    }

    public async Task<File> GetFileOrThrowAsync(Guid fileId)
    {
        var file = await _fileRepository.GetFileByIdAsync(fileId);
        if (file == null) throw new ServiceException(StatusCodes.Status404NotFound, "File not found");
        return file;
    }

    public void ValidateOwnership(File file, User user)
    {
        if (file.Owners?.All(u => u.Id != user.Id) ?? true)
            throw new ServiceException(StatusCodes.Status403Forbidden, "Forbidden access to file");
    }

    public void ValidateUploader(File file, User user)
    {
        if (file.UploaderId != user.Id)
            throw new ServiceException(StatusCodes.Status403Forbidden, "Only the uploader can perform this action");
    }

    public async Task<TaskVault.DataAccess.Entities.Task> GetTaskOrThrowAsync(Guid taskId)
    {
        var task = await _tasksRepository.GetTaskByIdAsync(taskId);
        if (task == null) throw new ServiceException(StatusCodes.Status404NotFound, "Task not found");
        return task;
    }

    public Task EnsureTaskAccessibleAsync(TaskVault.DataAccess.Entities.Task task, User user)
    {
        if (task.OwnerId != user.Id && task.Assignees?.All(a => a.Id != user.Id) == true)
            throw new ServiceException(StatusCodes.Status403Forbidden, "Access forbidden");
        return Task.CompletedTask;
    }

    public Task EnsureTaskOwnerAsync(TaskVault.DataAccess.Entities.Task task, User user)
    {
        if (task.OwnerId != user.Id)
            throw new ServiceException(StatusCodes.Status403Forbidden, "Access forbidden");
        return Task.CompletedTask;
    }
    
    public Task EnsureFileOwnerAsync(TaskVault.DataAccess.Entities.File file, User user)
    {
        if (file.UploaderId != user.Id)
            throw new ServiceException(StatusCodes.Status403Forbidden, "This file or directory doesn't belong to you");
        return Task.CompletedTask;
    }

    public Task ValidateTaskItemFileCompatibilityAsync(TaskItem taskItem, File file)
    {
        if (taskItem.FileTypeId != file.FileTypeId)
            throw new ServiceException(StatusCodes.Status400BadRequest,
                $"File with id {file.Id} should have {file.FileType?.Extension} format");
        return Task.CompletedTask;
    }

    public Task EnsureUserCannotUploadMoreThanMaxFilesAtOnceAsync(UploadFileDto uploadFileDto)
    {
        if (uploadFileDto.Files.Count() > _fileUploadOptions.MaxNoOfFilesToUploadOnce)
            throw new ServiceException(StatusCodes.Status400BadRequest,
                $"You can upload up to {_fileUploadOptions.MaxNoOfFilesToUploadOnce} at once");
        return Task.CompletedTask;
    }
}