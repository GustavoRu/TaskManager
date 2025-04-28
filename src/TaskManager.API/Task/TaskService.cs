
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManager.API.Shared.Utils;
using TaskManager.API.Task.DTOs;
using TaskManager.API.Task.Models;
using TaskManager.API.Task;

namespace TaskManager.API.Task
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserContextAccessor _userContextAccessor;

        public TaskService(ITaskRepository taskRepository, IUserContextAccessor userContextAccessor)
        {
            _taskRepository = taskRepository;
            _userContextAccessor = userContextAccessor;
        }

        public async Task<List<TaskResponseDto>> GetAllTasksAsync()
        {
            var tasks = await _taskRepository.GetAllTasksAsync();
            return tasks.Select(t => new TaskResponseDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                OwnerId = t.OwnerId
            }).ToList();
        }

        public async Task<TaskResponseDto> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            return task != null ? MapToTaskResponseDto(task) : null;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(TaskPostDto taskDto)
        {
            var userId = _userContextAccessor.GetCurrentUserId();
            var taskModel = new TaskModel
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                OwnerId = userId,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdTask = await _taskRepository.CreateTaskAsync(taskModel);
            await CreateTaskHistoryEntryAsync(createdTask.TaskId, userId, TaskAction.Created);
            return MapToTaskResponseDto(createdTask);
        }

        public async Task<bool> UpdateTaskAsync(int id, TaskUpdateDto taskDto)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null)
                return false;

            var oldValues = new { task.Title, task.Description, task.IsCompleted };

            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.IsCompleted = taskDto.IsCompleted;
            task.UpdatedAt = DateTime.UtcNow;
            
            var result = await _taskRepository.UpdateTaskAsync(task);
            if(result)
            {
                var userId = _userContextAccessor.GetCurrentUserId();
                var changes = new
                {
                    OldValues = oldValues,
                    NewValues = new { taskDto.Title, taskDto.Description, taskDto.IsCompleted }
                };

                await CreateTaskHistoryEntryAsync(id, userId, TaskAction.Updated, changes);
            }

            return result;                 
        }

        public async Task<List<TaskResponseDto>> GetCurrentUserTasksAsync()
        {
            var userId = _userContextAccessor.GetCurrentUserId();
            var tasks = await _taskRepository.GetTasksByUserIdAsync(userId);
            return tasks.Select(MapToTaskResponseDto).ToList();
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var result = await _taskRepository.DeleteTaskAsync(id);
            // if (result)
            // {
            //     var userId = _userContextAccessor.GetCurrentUserId();
            //     await CreateTaskHistoryEntryAsync(id, userId, TaskAction.Deleted);
            // }
            return result;
        }

        

        public async System.Threading.Tasks.Task CreateTaskHistoryEntryAsync(int taskId, int userId, TaskAction action, object changes = null)
        {
            var historyEntry = new TaskHistoryModel
            {
                TaskId = taskId,
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                ChangesJson = changes != null ? JsonSerializer.Serialize(changes) : JsonSerializer.Serialize(new { })
            };

            await _taskRepository.AddTaskHistoryAsync(historyEntry);
        }
        private static TaskResponseDto MapToTaskResponseDto(TaskModel task)
        {
            return new TaskResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                OwnerId = task.OwnerId
            };
        }
    }
}