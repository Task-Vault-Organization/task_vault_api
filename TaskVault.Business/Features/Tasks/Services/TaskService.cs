using AutoMapper;
using Microsoft.AspNetCore.Http;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Contracts.Features.Tasks.Abstractions;
using TaskVault.Contracts.Features.Tasks.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using Task = TaskVault.DataAccess.Entities.Task;

namespace TaskVault.Business.Features.Tasks.Services;

public class TaskService : ITaskService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IRepository<User> _userRepository;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly IMapper _mapper;
    private readonly IFileRepository _fileRepository;
    private readonly ITaskSubmissionTaskItemFileRepository _taskSubmissionTaskItemFileRepository;
    private readonly ITaskSubmissionRepository _taskSubmissionRepository;

    public TaskService(IExceptionHandlingService exceptionHandlingService, IRepository<User> userRepository, ITaskItemRepository taskItemRepository, ITasksRepository tasksRepository, IMapper mapper, IFileRepository fileRepository, ITaskSubmissionTaskItemFileRepository taskSubmissionTaskItemFileRepository, ITaskSubmissionRepository taskSubmissionRepository)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _userRepository = userRepository;
        _taskItemRepository = taskItemRepository;
        _tasksRepository = tasksRepository;
        _mapper = mapper;
        _fileRepository = fileRepository;
        _taskSubmissionTaskItemFileRepository = taskSubmissionTaskItemFileRepository;
        _taskSubmissionRepository = taskSubmissionRepository;
    }

    public async Task<BaseApiResponse> CreateTaskAsync(string userEmail, CreateTaskDto createTask)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var taskId = Guid.NewGuid();
            var newTask = Task.Create(taskId, createTask.Title, createTask.Description, foundUser.Id);
            if (createTask.DeadlineAt != null) newTask.DeadlineAt = createTask.DeadlineAt;
            AddAssignees(createTask.AssigneesIds, newTask);
            
            await _tasksRepository.AddAsync(newTask);
            
            AddTaskItemsAsync(createTask.TaskItems, taskId);
            
            return BaseApiResponse.Create("Successfully added new task");
        }, "Error when creating task");
    }

    public async Task<GetTasksResponseDto> GetOwnedTasksAsync(string userEmail)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var tasks = await _tasksRepository.GetOwnedTasksAsync(foundUser.Id);
            var enumerable = tasks as Task[] ?? tasks.ToArray();
            if (!enumerable.Any())
            {
                return GetTasksResponseDto.Create("No owned tasks found", Enumerable.Empty<GetTaskDto>());
            }

            var taskDtos = await System.Threading.Tasks.Task.WhenAll(enumerable.Select(async t =>
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
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var tasks = await _tasksRepository.GetAssignedTasksAsync(foundUser.Id);
            var enumerable = tasks as Task[] ?? tasks.ToArray();
            if (!enumerable.Any())
            {
                return GetTasksResponseDto.Create("No owned tasks found", Enumerable.Empty<GetTaskDto>());
            }

            var taskDtos = await System.Threading.Tasks.Task.WhenAll(enumerable.Select(async t =>
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
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var task = await _tasksRepository.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "Task not found");
            }

            if (task.OwnerId != foundUser.Id && !task.Assignees!.Any(u => u.Id == foundUser.Id))
            {
                throw new ServiceException(StatusCodes.Status403Forbidden, "Task forbidden");
            }

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
            var foundUser = (await _userRepository.FindAsync(u => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            var task = await _tasksRepository.GetTaskByIdAsync(createTaskSubmissionDto.TaskId);
            if (task == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "Task not found");
            }

            if (!task.Assignees!.Any(u => u.Id == foundUser.Id))
            {
                throw new ServiceException(StatusCodes.Status403Forbidden, "Task forbidden");
            }

            var taskItems = await _taskItemRepository.GetTaskItemOfTaskAsync(task.Id);

            var enumerable = taskItems as TaskItem[] ?? taskItems.ToArray();
            foreach (var taskItem in enumerable)
            {
                var foundTask = createTaskSubmissionDto.TaskItemFiles.Where(tif => tif.TaskItemId == taskItem.Id);
                if (!foundTask.Any()) throw new ServiceException(StatusCodes.Status400BadRequest, $"Task with title '{taskItem.Title}' is missing");
            }

            foreach (var getTaskItemFileDto in createTaskSubmissionDto.TaskItemFiles)
            {
                var foundTask = enumerable.Where(ti => ti.Id == getTaskItemFileDto.TaskItemId);
                var items = foundTask as TaskItem[] ?? foundTask.ToArray();
                if (!items.Any()) throw new ServiceException(StatusCodes.Status404NotFound, "Task not found");

                var foundTaskItem = items.FirstOrDefault();

                var foundFile = await _fileRepository.GetFileByIdAsync(getTaskItemFileDto.FileId);
                if (foundFile == null)
                {
                    throw new ServiceException(StatusCodes.Status404NotFound,
                        $"File with id {getTaskItemFileDto.FileId} not found");
                }

                if (foundTaskItem != null && foundTaskItem.FileTypeId != foundFile.FileTypeId)
                {
                    throw new ServiceException(StatusCodes.Status400BadRequest,
                        $"File with id {getTaskItemFileDto.FileId} should have {foundFile.FileType?.Extension} format");
                }
            }

            var submissionId = Guid.NewGuid();
            var newSubmission = TaskSubmission.Create(submissionId, task.Id, foundUser.Id);

            await _taskSubmissionRepository.AddAsync(newSubmission);
            
            foreach (var getTaskItemFileDto in createTaskSubmissionDto.TaskItemFiles)
            {
                var newTaskItemFile = TaskSubmissionTaskItemFile.Create(submissionId, getTaskItemFileDto.TaskItemId,
                    getTaskItemFileDto.FileId);

                await _taskSubmissionTaskItemFileRepository.AddAsync(newTaskItemFile);
            }

            return BaseApiResponse.Create("Succesfully submitted submission");
        }, "Error when retrieving task");
    }

    private async void AddTaskItemsAsync(IEnumerable<CreateTaskItemDto> taskItems, Guid taskId)
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