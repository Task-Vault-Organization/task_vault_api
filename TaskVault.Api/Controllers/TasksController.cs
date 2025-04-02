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
    public async Task<IActionResult> GetOwnedTasksAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetOwnedTasksAsync(userEmail));
    }
    
    [HttpGet("assigned")]
    public async Task<IActionResult> GetAssignedTasksAsync()
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetAssignedTasksAsync(userEmail));
    }
    
    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTaskAsync(Guid taskId)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.GetTaskAsync(userEmail, taskId));
    }
    
    [HttpPost("submit")]
    public async Task<IActionResult> CreateTaskSubmissionAsync([FromBody] CreateTaskSubmissionDto createTaskSubmissionDto)
    {
        var userEmail = AuthorizationHelper.GetUserEmailFromClaims(User);
        return Ok(await _taskService.CreateTaskSubmissionAsync(userEmail, createTaskSubmissionDto));
    }
}