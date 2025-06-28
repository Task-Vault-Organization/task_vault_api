using TaskVault.Contracts.Features.Tasks.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Abstractions;

public interface ITaskService
{
    Task<BaseApiResponse> CreateTaskAsync(string userEmail, CreateTaskDto createTask);
    Task<GetOwnedTasksResponseDto> GetOwnedTasksAsync(string userEmail, string sortBy, string filterBy);
    Task<GetAssignedTasksResponseDto> GetAssignedTasksAsync(string userEmail, string sortBy, string filterBy);
    Task<GetTaskResponseDto> GetTaskAsync(string userEmail, Guid taskId);
    Task<GetOwnedTaskResponseDto> GetOwnedTaskAsync(string userEmail, Guid taskId);
    Task<GetAssignedTaskResponseDto> GetAssignedTaskAsync(string userEmail, Guid taskId);
    Task<BaseApiResponse> CreateTaskSubmissionAsync(string userEmail, CreateTaskSubmissionDto createTaskSubmissionDto);
    Task<GetTaskSubmissionsResponseDto> GetTaskSubmissionsAsync(string userEmail, Guid taskId);

    Task<GetTaskSubmissionsResponseDto> GetTaskSubmissionsForAssigneeAsync(string userEmail, Guid taskId,
        Guid assigneeId);

    Task<BaseApiResponse> ResolveTaskSubmissionAsync(string userEmail, ResolveTaskSubmissionDto resolveTaskSubmissionDto);
}