using System.Xml;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TaskVault.Business.Features.Notifications.Models;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Contracts.Features.FileStorage.Dtos;
using TaskVault.Contracts.Features.Notifications.Abstractions;
using TaskVault.Contracts.Features.Tasks.Abstractions;
using TaskVault.Contracts.Features.Tasks.Dtos;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.Contracts.Shared.Validator.Abstractions;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;
using File = TaskVault.DataAccess.Entities.File;
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
    private readonly ITaskSubmissionTaskItemFileCommentRepository _commentRepository;
    private readonly IFileRepository _fileRepository;
    private readonly INotificationsService _notificationsService;

    public TaskService(
        IExceptionHandlingService exceptionHandlingService,
        ITaskItemRepository taskItemRepository,
        ITasksRepository tasksRepository,
        IMapper mapper,
        ITaskSubmissionTaskItemFileRepository taskSubmissionTaskItemFileRepository,
        ITaskSubmissionRepository taskSubmissionRepository,
        IEntityValidator entityValidator, IRepository<User> userRepository, ITaskSubmissionTaskItemFileCommentRepository commentRepository, IFileRepository fileRepository, INotificationsService notificationsService)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _taskItemRepository = taskItemRepository;
        _tasksRepository = tasksRepository;
        _mapper = mapper;
        _taskSubmissionTaskItemFileRepository = taskSubmissionTaskItemFileRepository;
        _taskSubmissionRepository = taskSubmissionRepository;
        _entityValidator = entityValidator;
        _userRepository = userRepository;
        _commentRepository = commentRepository;
        _fileRepository = fileRepository;
        _notificationsService = notificationsService;
    }

    public async Task<BaseApiResponse> CreateTaskAsync(string userEmail, CreateTaskDto createTask)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var taskId = Guid.NewGuid();
            var newTask = Task.Create(taskId, createTask.Title, createTask.Description, user.Id);
            if (createTask.DeadlineAt != null) newTask.DeadlineAt = createTask.DeadlineAt;

            await AddAssignees(createTask.AssigneesEmails, newTask);
            await _tasksRepository.AddAsync(newTask);
            AddTaskItems(createTask.TaskItems, taskId);

            return BaseApiResponse.Create("Successfully added new task");
        }, "Error when creating task");
    }

    public async Task<GetOwnedTasksResponseDto> GetOwnedTasksAsync(string userEmail, string sortBy, string filterBy)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var tasks = await _tasksRepository.GetOwnedTasksAsync(user.Id);

            filterBy = filterBy.ToLower() ?? "all";
            tasks = filterBy switch
            {
                "started" => tasks.Where(t => t.Status?.Name == "Started"),
                "completed" => tasks.Where(t => t.Status?.Name == "Completed"),
                "all" => tasks,
                _ => tasks
            };

            sortBy = sortBy.ToLower();
            tasks = sortBy switch
            {
                "oldest" => tasks.OrderBy(t => t.CreatedAt),
                _ => tasks.OrderByDescending(t => t.CreatedAt)
            };

            var taskDtos = await System.Threading.Tasks.Task.WhenAll(tasks.Select(t =>
            {
                var dto = _mapper.Map<GetOwnedTaskDto>(t);

                dto.Owner = _mapper.Map<GetUserDto>(t.Owner);

                var assigneeDtos = t.Assignees?.Select(assignee =>
                {
                    var submission =
                        (_taskSubmissionRepository.Find(ts =>
                            ts.TaskId == t.Id && ts.SubmittedById == assignee.Id)).FirstOrDefault();
                    return new GetTaskSubmissionUserDto
                    {
                        Id = assignee.Id,
                        Email = assignee.Email,
                        RootDirectoryId = assignee.RootDirectoryId,
                        Approved = submission?.Approved,
                    };
                }) ?? Enumerable.Empty<GetTaskSubmissionUserDto>();

                dto.Assignees = assigneeDtos;

                return System.Threading.Tasks.Task.FromResult(dto);
            }));

            return GetOwnedTasksResponseDto.Create("Successfully retrieved owned tasks", taskDtos);
        }, "Error when retreieving owned tasks");
    }

    public async Task<GetAssignedTasksResponseDto> GetAssignedTasksAsync(string userEmail, string sortBy, string filterBy)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var tasks = await _tasksRepository.GetAssignedTasksAsync(user.Id);

            filterBy = filterBy?.ToLower() ?? "all";
            tasks = filterBy switch
            {
                "started" => tasks.Where(t => t.Status?.Name == "Started"),
                "completed" => tasks.Where(t => t.Status?.Name == "Completed"),
                "all" => tasks,
                _ => tasks
            };

            sortBy = sortBy.ToLower();
            tasks = sortBy switch
            {
                "oldest" => tasks.OrderBy(t => t.CreatedAt),
                _ => tasks.OrderByDescending(t => t.CreatedAt)
            };

            var taskDtos = await System.Threading.Tasks.Task.WhenAll(tasks.Select(async t =>
            {
                var dto = _mapper.Map<GetAssignedTaskDto>(t);

                dto.Owner = _mapper.Map<GetUserDto>(t.Owner);

                var submission =
                    (await _taskSubmissionRepository.FindAsync((ts) => ts.TaskId == t.Id && ts.SubmittedById == user.Id)).FirstOrDefault();
                var comments =
                    await _commentRepository.FindAsync(c => submission != null && c.TaskSubmissionId == submission.Id);

                dto.NoComments = comments.Count();
                dto.Approved = submission?.Approved;

                return dto;
            }));

            return GetAssignedTasksResponseDto.Create("Successfully retrieved assigned tasks", taskDtos);
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
    
    public async Task<GetOwnedTaskResponseDto> GetOwnedTaskAsync(string userEmail, Guid taskId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var task = await _entityValidator.GetTaskOrThrowAsync(taskId);
            await _entityValidator.EnsureTaskOwnerAsync(task, user);

            var dto = _mapper.Map<GetOwnedTaskDto>(task);
            var taskItems = await _taskItemRepository.GetTaskItemOfTaskAsync(task.Id);
            dto.TaskItems = taskItems.Select(ti => _mapper.Map<GetTaskItemDto>(ti));

            var assigneeDtos = task.Assignees?.Select(assignee =>
            {
                var submission =
                    _taskSubmissionRepository.Find(TS => TS.TaskId == task.Id && TS.SubmittedById == assignee.Id).FirstOrDefault();
                return new GetTaskSubmissionUserDto
                {
                    Id = assignee.Id,
                    Email = assignee.Email,
                    RootDirectoryId = assignee.RootDirectoryId,
                    Approved = submission?.Approved,
                };
            }) ?? Enumerable.Empty<GetTaskSubmissionUserDto>();

            dto.Assignees = assigneeDtos;

            return GetOwnedTaskResponseDto.Create("Successfully retrieved owned task", dto);
        }, "Error when retrieving owned task");
    }

    public async Task<GetAssignedTaskResponseDto> GetAssignedTaskAsync(string userEmail, Guid taskId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var task = await _entityValidator.GetTaskOrThrowAsync(taskId);
            await _entityValidator.EnsureTaskAccessibleAsync(task, user);

            var dto = _mapper.Map<GetAssignedTaskDto>(task);
            var taskItems = await _taskItemRepository.GetTaskItemOfTaskAsync(task.Id);

            TaskSubmission? taskSubmission = null;
            if (task.TaskSubmissions != null)
            {
                taskSubmission = (await _taskSubmissionRepository.FindAsync(
                    ts => ts.TaskId == task.Id && ts.SubmittedById == user.Id)).FirstOrDefault();
            }

            var tsFiles = taskSubmission != null
                ? (await _taskSubmissionTaskItemFileRepository.FindAsync(t => t.TaskSubmissionId == taskSubmission.Id)).ToList()
                : new List<TaskSubmissionTaskItemFile>();

            dto.TaskItems = (await System.Threading.Tasks.Task.WhenAll(taskItems.Select(async ti =>
            {
                var tsFile = tsFiles.FirstOrDefault(t => t.TaskItemId == ti.Id);
                File? file = tsFile?.File;

                var comments = tsFile != null
                    ? (await _commentRepository.FindAsync(c =>
                          c.TaskSubmissionId == taskSubmission.Id && c.TaskSubmissionTaskItemFileId == tsFile.FileId))
                        .Select(c => _mapper.Map<GetTaskSubmittedItemFileCommentDto>(c))
                    : [];

                var getFile = _mapper.Map<GetFileDto>(file);

                return new GetAssignedTaskItemDto
                {
                    Id = ti.Id,
                    Title = ti.Title,
                    Description = ti.Description,
                    TaskId = ti.TaskId,
                    FileTypeId = ti.FileTypeId,
                    FileCategoryId = ti.FileCategoryId,
                    FileType = ti.FileType,
                    FileCategory = ti.FileCategory,
                    SubmittedFile = getFile,
                    Comments = comments
                };
            }))).ToList();
            
            dto.Approved = taskSubmission?.Approved;
            dto.DissaprovedComment = taskSubmission?.DissaproveComment;

            return GetAssignedTaskResponseDto.Create("Successfully retrieved assigned task", dto);
        }, "Error when retrieving assigned task");
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

            var existingSubmission = (await _taskSubmissionRepository.FindAsync(
                ts => ts.TaskId == task.Id && ts.SubmittedById == user.Id)).FirstOrDefault();

            Guid submissionId;

            if (existingSubmission != null)
            {
                submissionId = existingSubmission.Id;

                var existingFiles = await _taskSubmissionTaskItemFileRepository
                    .FindAsync(f => f.TaskSubmissionId == submissionId);

                foreach (var file in existingFiles)
                {
                    await _taskSubmissionTaskItemFileRepository.RemoveAsync(file);
                }

                existingSubmission.Approved = null;
                existingSubmission.DissaproveComment = null;
                await _taskSubmissionRepository.UpdateAsync(existingSubmission, existingSubmission.Id);
            }
            else
            {
                submissionId = Guid.NewGuid();
                var submission = TaskSubmission.Create(submissionId, task.Id, user.Id);
                submission.Approved = null;
                await _taskSubmissionRepository.AddAsync(submission);
            }

            foreach (var dto in createTaskSubmissionDto.TaskItemFiles)
            {
                var itemFile = TaskSubmissionTaskItemFile.Create(submissionId, dto.TaskItemId, dto.FileId);
                await _taskSubmissionTaskItemFileRepository.AddAsync(itemFile);

                var file = await _entityValidator.GetFileOrThrowAsync(dto.FileId);
                var owner = task.Owner;

                var owners = file.Owners?.ToList() ?? new List<User>();

                if (owner != null && !owners.Any(o => o.Id == owner.Id))
                {
                    owners.Add(owner);
                    file.Owners = owners;
                    await _fileRepository.UpdateAsync(file, file.Id);
                }
            }

            var notificationContent = new GeneralInfoNotificationContent()
            {
                Message = $"{user.FullName} added new submission to task '{task.Title}'",
                User = _mapper.Map<GetUserDto>(user),
                TargetLink = $"task/owned/{task.Id}"
            };

            var notificationJson = JsonConvert.SerializeObject(notificationContent);
            var notification = Notification.Create(Guid.NewGuid(), task.OwnerId, DateTime.Now, notificationJson, 2, 1);

            await _notificationsService.SendAndSaveNotificationAsync(user.Id, notification);

            return BaseApiResponse.Create("Successfully submitted task submission");
        }, "Error when creating/updating task submission");
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
                var files = submissionFiles.Select(tf =>
                {
                    var file = tf.File;
                    return new GetTaskSubmissionFileDto
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        UploaderId = file.UploaderId,
                        UploadedAt = file.UploadedAt,
                        FileTypeId = file.FileTypeId,
                        IsDirectory = file.IsDirectory,
                        FileType = file.FileType,
                        Uploader = _mapper.Map<GetUserDto>(file.Uploader),
                        Owners = [],
                        TaskItemId = tf.TaskItemId
                    };
                });
                dto.TaskItemFiles = files;
                submissionDtos.Add(dto);
            }

            return GetTaskSubmissionsResponseDto.Create("Successfully retrieved submissions", submissionDtos);
        }, "Error when retrieving task submissions");
    }

    public async Task<GetTaskSubmissionsResponseDto> GetTaskSubmissionsForAssigneeAsync(string userEmail, Guid taskId, Guid assigneeId)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var task = await _entityValidator.GetTaskOrThrowAsync(taskId);
            await _entityValidator.EnsureTaskOwnerAsync(task, user);

            var submissions = await _taskSubmissionRepository.FindAsync(ts => ts.TaskId == taskId && ts.SubmittedById == assigneeId);
            var submissionDtos = new List<GetTaskSubmissionDto>();

            foreach (var sub in submissions)
            {
                var dto = _mapper.Map<GetTaskSubmissionDto>(sub);
                var submissionFiles = await _taskSubmissionTaskItemFileRepository.FindAsync(tf => tf.TaskSubmissionId == sub.Id);
                var files = submissionFiles.Select(tf =>
                {
                    var file = tf.File;
                    return new GetTaskSubmissionFileDto
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        UploaderId = file.UploaderId,
                        UploadedAt = file.UploadedAt,
                        FileTypeId = file.FileTypeId,
                        IsDirectory = file.IsDirectory,
                        FileType = file.FileType,
                        Uploader = _mapper.Map<GetUserDto>(file.Uploader),
                        Owners = [],
                        TaskItemId = tf.TaskItemId
                    };
                });
                dto.TaskItemFiles = files;
                dto.DissaproveComment = sub?.DissaproveComment;
                submissionDtos.Add(dto);
            }

            return GetTaskSubmissionsResponseDto.Create("Successfully retrieved assignee's submissions", submissionDtos);
        }, "Error when retrieving task submissions for assignee");
    }

    public async Task<BaseApiResponse> ResolveTaskSubmissionAsync(string userEmail, ResolveTaskSubmissionDto resolveTaskSubmissionDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var user = await _entityValidator.GetUserOrThrowAsync(userEmail);
            var taskSubmission = await _taskSubmissionRepository.GetTaskSubmissionByIdAsync(resolveTaskSubmissionDto.SubmissionId);
            if (taskSubmission == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "Task submission not found");
            }

            if (taskSubmission.Task != null && taskSubmission.Task.OwnerId != user.Id)
            {
                throw new ServiceException(StatusCodes.Status403Forbidden, "Forbidden access to task");
            }

            taskSubmission.Approved = resolveTaskSubmissionDto.IsApproved;
            if (!string.IsNullOrEmpty(resolveTaskSubmissionDto.DissaproveComment))
            {
                taskSubmission.DissaproveComment = resolveTaskSubmissionDto.DissaproveComment;
            }

            await _taskSubmissionRepository.UpdateAsync(taskSubmission, taskSubmission.Id);

            GeneralInfoNotificationContent notificationContent = null!;

            if (resolveTaskSubmissionDto.IsApproved)
            {
                notificationContent = new GeneralInfoNotificationContent()
                {
                    Message = $"{user.FullName} just approved your submission to the task '{taskSubmission.Task.Title}'",
                    User = _mapper.Map<GetUserDto>(user),
                    TargetLink = $"task/assigned/{taskSubmission.Task.Id}"
                };
            }
            else
            {
                notificationContent = new GeneralInfoNotificationContent()
                {
                    Message = $"{user.FullName} reject your submission to '{taskSubmission.Task.Title}' Click here to see the reason",
                    User = _mapper.Map<GetUserDto>(user),
                    TargetLink = $"task/assigned/{taskSubmission.Task.Id}"
                };
            }

            var notificationJson = JsonConvert.SerializeObject(notificationContent);
            var notification = Notification.Create(Guid.NewGuid(), taskSubmission.SubmittedById, DateTime.Now, notificationJson, 2, 1);

            await _notificationsService.SendAndSaveNotificationAsync(user.Id, notification);

            return BaseApiResponse.Create("Successfully resolved task submission");
        }, "Error when resolving task submission");
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

    private async System.Threading.Tasks.Task AddAssignees(IEnumerable<string>? assigneesEmails, Task newTask)
    {
        if (assigneesEmails != null)
        {
            var uniqueEmails = assigneesEmails
                .Select(email => email.Trim().ToLower())
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct();

            var users = await _userRepository.FindAsync(u => uniqueEmails.Contains(u.Email.ToLower()));
            var userDict = users.ToDictionary(u => u.Email.ToLower(), u => u);

            var missingEmails = uniqueEmails.Except(userDict.Keys).ToList();
            if (missingEmails.Any())
            {
                throw new ServiceException(StatusCodes.Status400BadRequest, $"The following users do not exist: {string.Join(", ", missingEmails)}");
            }

            foreach (var user in users)
            {
                if (newTask.Assignees == null)
                {
                    newTask.Assignees = new List<User> { user };
                }
                else
                {
                    var assignees = newTask.Assignees.ToList();
                    assignees.Add(user);
                    newTask.Assignees = assignees;
                }

                if (user.Tasks == null)
                {
                    user.Tasks = new List<Task> { newTask };
                }
                else
                {
                    var tasks = user.Tasks.ToList();
                    tasks.Add(newTask);
                    user.Tasks = tasks;
                }
            }
        }
    }
}