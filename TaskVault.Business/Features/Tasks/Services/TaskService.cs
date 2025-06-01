using AutoMapper;
using Microsoft.AspNetCore.Http;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Contracts.Features.Tasks.Abstractions;
using TaskVault.Contracts.Features.Tasks.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using Task = TaskVault.DataAccess.Entities.Task;

namespace TaskVault.Business.Features.Tasks.Services;

public class TaskService : ITaskService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly IMapper _mapper;
    private readonly ITaskSubmissionTaskItemFileRepository _taskSubmissionTaskItemFileRepository;
    private readonly ITaskSubmissionRepository _taskSubmissionRepository;
    private readonly IEntityValidator _entityValidator;
    private readonly IRepository<User> _userRepository;

    public TaskService(
        IExceptionHandlingService exceptionHandlingService,
        ITaskItemRepository taskItemRepository,
        ITasksRepository tasksRepository,
        IMapper mapper,
        ITaskSubmissionTaskItemFileRepository taskSubmissionTaskItemFileRepository,
        ITaskSubmissionRepository taskSubmissionRepository,
        IEntityValidator entityValidator, IRepository<User> userRepository)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _taskItemRepository = taskItemRepository;
        _tasksRepository = tasksRepository;
        _mapper = mapper;
        _taskSubmissionTaskItemFileRepository = taskSubmissionTaskItemFileRepository;
        _taskSubmissionRepository = taskSubmissionRepository;
        _entityValidator = entityValidator;
        _userRepository = userRepository;
    }

    public async Task<BaseApiResponse> CreateTaskAsync(string userEmail, CreateTaskDto createTask)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var taskId = Guid.NewGuid();
            var newTask = Task.Create(taskId, createTask.Title, createTask.Description, user.Id);
            if (createTask.DeadlineAt != null) newTask.DeadlineAt = createTask.DeadlineAt;

            AddAssignees(createTask.AssigneesIds, newTask);
            await _tasksRepository.AddAsync(newTask);
            AddTaskItems(createTask.TaskItems, taskId);

            return BaseApiResponse.Create("Successfully added new task");
        }, "Error when creating task");
    }

    public async Task<GetTasksResponseDto> GetOwnedTasksAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var tasks = await _tasksRepository.GetOwnedTasksAsync(user.Id);

            var taskDtos = await System.Threading.Tasks.Task.WhenAll(tasks.Select(async t =>
            {
                var getTask = _mapper.Map<GetTaskDto>(t);
                var taskItems = await _taskItemRepository.GetTaskItemOfTaskAsync(t.Id);
                getTask.TaskItems = taskItems.Select(ti => _mapper.Map<GetTaskItemDto>(ti));
                return getTask;
            }));

            return GetTasksResponseDto.Create("Successfully retrieved owned tasks", taskDtos);
        }, "Error when retrieving owned tasks");
    }

    public async Task<GetTasksResponseDto> GetAssignedTasksAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var tasks = await _tasksRepository.GetAssignedTasksAsync(user.Id);

            var taskDtos = await System.Threading.Tasks.Task.WhenAll(tasks.Select(async t =>
            {
                var getTask = _mapper.Map<GetTaskDto>(t);
                var taskItems = await _taskItemRepository.GetTaskItemOfTaskAsync(t.Id);
                getTask.TaskItems = taskItems.Select(ti => _mapper.Map<GetTaskItemDto>(ti));
                return getTask;
            }));

            return GetTasksResponseDto.Create("Successfully retrieved assigned tasks", taskDtos);
        }, "Error when retrieving assigned tasks");
    }

    public async Task<GetTaskResponseDto> GetTaskAsync(string userEmail, Guid taskId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var task = await _entityValidator.GetTaskOrThrowAsync(taskId);
            await _entityValidator.EnsureTaskAccessibleAsync(task, user);

            var getTask = _mapper.Map<GetTaskDto>(task);
            var taskItems = await _taskItemRepository.GetTaskItemOfTaskAsync(task.Id);
            getTask.TaskItems = taskItems.Select(ti => _mapper.Map<GetTaskItemDto>(ti));

            return GetTaskResponseDto.Create("Succesfully retrieved task", getTask);
        }, "Error when retrieving task");
    }

    public async Task<BaseApiResponse> CreateTaskSubmissionAsync(string userEmail, CreateTaskSubmissionDto createTaskSubmissionDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var task = await _entityValidator.GetTaskOrThrowAsync(createTaskSubmissionDto.TaskId);
            await _entityValidator.EnsureTaskAccessibleAsync(task, user);

            var taskItems = await _taskItemRepository.GetTaskItemOfTaskAsync(task.Id);
            var itemList = taskItems.ToList();

            foreach (var taskItem in itemList)
            {
                var hasSubmission = createTaskSubmissionDto.TaskItemFiles.Any(tif => tif.TaskItemId == taskItem.Id);
                if (!hasSubmission)
                    throw new ServiceException(StatusCodes.Status400BadRequest, $"Task with title '{taskItem.Title}' is missing");
            }

            foreach (var dto in createTaskSubmissionDto.TaskItemFiles)
            {
                var taskItem = itemList.FirstOrDefault(ti => ti.Id == dto.TaskItemId)
                    ?? throw new ServiceException(StatusCodes.Status404NotFound, "Task not found");

                var file = await _entityValidator.GetFileOrThrowAsync(dto.FileId);
                await _entityValidator.ValidateTaskItemFileCompatibilityAsync(taskItem, file);
            }

            var submissionId = Guid.NewGuid();
            var submission = TaskSubmission.Create(submissionId, task.Id, user.Id);
            await _taskSubmissionRepository.AddAsync(submission);

            foreach (var dto in createTaskSubmissionDto.TaskItemFiles)
            {
                var itemFile = TaskSubmissionTaskItemFile.Create(submissionId, dto.TaskItemId, dto.FileId);
                await _taskSubmissionTaskItemFileRepository.AddAsync(itemFile);
            }

            return BaseApiResponse.Create("Succesfully submitted submission");
        }, "Error when retrieving task");
    }

    public async Task<GetTaskSubmissionsResponseDto> GetTaskSubmissionsAsync(string userEmail, Guid taskId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var task = await _entityValidator.GetTaskOrThrowAsync(taskId);
            await _entityValidator.EnsureTaskOwnerAsync(task, user);

            var submissions = await _taskSubmissionRepository.FindAsync(ts => ts.TaskId == taskId);
            var submissionDtos = new List<GetTaskSubmissionDto>();

            foreach (var submission in submissions)
            {
                var dto = _mapper.Map<GetTaskSubmissionDto>(submission);
                var submissionFiles = await _taskSubmissionTaskItemFileRepository.FindAsync(tf => tf.TaskSubmissionId == submission.Id);
                dto.TaskItemFiles = submissionFiles.Select(f => _mapper.Map<GetTaskItemFileDto>(f));
                submissionDtos.Add(dto);
            }

            return GetTaskSubmissionsResponseDto.Create("Successfully retrieved submissions", submissionDtos);
        }, "Error when retrieving task submissions");
    }


    private async void AddTaskItems(IEnumerable<CreateTaskItemDto> taskItems, Guid taskId)
    {
        foreach (var createTaskItemDto in taskItems)
        {
            var newTaskItem = TaskItem.Create(createTaskItemDto.Title, createTaskItemDto.FileTypeId,
                createTaskItemDto.FileCategoryId, taskId);
            if (createTaskItemDto.Description != null) newTaskItem.Description = createTaskItemDto.Description;

            await _taskItemRepository.AddAsync(newTaskItem);
        }
    }

    private async void AddAssignees(IEnumerable<Guid>? assigneesIds, Task newTask)
    {
        if (assigneesIds != null)
            foreach (var assigneesId in assigneesIds)
            {
                var foundAssignee = (await _userRepository.FindAsync(u => u.Id == assigneesId)).FirstOrDefault();
                if (foundAssignee != null)
                {
                    if (newTask.Assignees == null)
                    {
                        newTask.Assignees = new List<User>() { foundAssignee };
                    }
                    else
                    {
                        newTask.Assignees.ToList().Add(foundAssignee);
                    }
                
                    if (foundAssignee.Tasks == null)
                    {
                        foundAssignee.Tasks = new List<Task>() { newTask };
                    }
                    else
                    {
                        foundAssignee.Tasks.ToList().Add(newTask);
                    }
                }
            }
    }
}