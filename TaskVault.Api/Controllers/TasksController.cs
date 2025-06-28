using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskVault.Api.Helpers;
using TaskVault.Contracts.Features.Tasks.Abstractions;
using TaskVault.Contracts.Features.Tasks.Dtos;

namespace TaskVault.Api.Controllers;

[Route("api/tasks")]
[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTaskAsync([FromBody] CreateTaskDto createTask)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.CreateTaskAsync(userEmail, createTask));
    }

    [HttpGet("owned")]
    public async Task<IActionResult> GetOwnedTasksAsync([FromQuery] string sortBy = "", [FromQuery] string filterBy = "")
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetOwnedTasksAsync(userEmail, sortBy, filterBy));
    }

    [HttpGet("assigned")]
    public async Task<IActionResult> GetAssignedTasksAsync([FromQuery] string sortBy = "", [FromQuery] string filterBy = "")
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetAssignedTasksAsync(userEmail, sortBy, filterBy));
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTaskAsync(Guid taskId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetTaskAsync(userEmail, taskId));
    }

    [HttpGet("{taskId}/owned")]
    public async Task<IActionResult> GetOwnedTaskAsync(Guid taskId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetOwnedTaskAsync(userEmail, taskId));
    }

    [HttpGet("{taskId}/assigned")]
    public async Task<IActionResult> GetAssignedTaskAsync(Guid taskId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetAssignedTaskAsync(userEmail, taskId));
    }

    [HttpGet("{taskId}/submissions")]
    public async Task<IActionResult> GetTaskSubmissionsAsync(Guid taskId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetTaskSubmissionsAsync(userEmail, taskId));
    }

    [HttpGet("{taskId}/submissions/{assigneeId}")]
    public async Task<IActionResult> GetTaskSubmissionsForAssigneeAsync(Guid taskId, Guid assigneeId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetTaskSubmissionsForAssigneeAsync(userEmail, taskId, assigneeId));
    }

    [HttpPost("submit")]
    public async Task<IActionResult> CreateTaskSubmissionAsync([FromBody] CreateTaskSubmissionDto createTaskSubmissionDto)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.CreateTaskSubmissionAsync(userEmail, createTaskSubmissionDto));
    }
    
    [HttpPost("resolve-submission")]
    public async Task<IActionResult> ResolveTaskSubmissionAsync([FromBody] ResolveTaskSubmissionDto resolveTaskSubmissionDto)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.ResolveTaskSubmissionAsync(userEmail, resolveTaskSubmissionDto));
    }
}