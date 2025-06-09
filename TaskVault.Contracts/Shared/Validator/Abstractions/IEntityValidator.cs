using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.DataAccess.Entities;
using File = TaskVault.DataAccess.Entities.File;
using Task = System.Threading.Tasks.Task;

namespace TaskVault.Contracts.Shared.Validator.Abstractions;

public interface IEntityValidator
{
    Task<User> GetUserOrThrowAsync(string email);
    Task<User> GetUserOrThrowAsync(Guid userId);
    Task EnsureUserDoesNotExistAsync(string email);
    Task<User> EnsureUserExistsAsync(string email);
    Task ValidatePasswordAsync(string rawPassword, User user);
    Task<File> GetFileOrThrowAsync(Guid fileId);
    void ValidateOwnership(File file, User user);
    void ValidateUploader(File file, User user);
    Task<TaskVault.DataAccess.Entities.Task> GetTaskOrThrowAsync(Guid taskId);
    Task EnsureTaskAccessibleAsync(TaskVault.DataAccess.Entities.Task task, User user);
    Task EnsureTaskOwnerAsync(TaskVault.DataAccess.Entities.Task task, User user);
    Task ValidateTaskItemFileCompatibilityAsync(TaskItem taskItem, File file);
    Task EnsureFileOwnerAsync(TaskVault.DataAccess.Entities.File file, User user);
    Task EnsureUserCannotUploadMoreThanMaxFilesAtOnceAsync(UploadFileDto uploadFileDto);
}