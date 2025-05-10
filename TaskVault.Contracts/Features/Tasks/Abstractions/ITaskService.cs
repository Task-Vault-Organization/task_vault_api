using TaskVault.Contracts.Features.Tasks.Dtos;
using TaskVault.Contracts.Shared.Dtos;

namespace TaskVault.Contracts.Features.Tasks.Abstractions;

public interface ITaskService
{
    Task<BaseApiResponse> CreateTaskAsync(string userEmail, CreateTaskDto createTask);
    Task<GetTasksResponseDto> GetOwnedTasksAsync(string userEmail);
    Task<GetTasksResponseDto> GetAssignedTasksAsync(string userEmail);
    Task<GetTaskResponseDto> GetTaskAsync(string userEmail, Guid taskId);
    Task<BaseApiResponse> CreateTaskSubmissionAsync(string userEmail, CreateTaskSubmissionDto createTaskSubmissionDto);
    Task<GetTaskSubmissionsResponseDto> GetTaskSubmissionsAsync(string userEmail, Guid taskId);
}