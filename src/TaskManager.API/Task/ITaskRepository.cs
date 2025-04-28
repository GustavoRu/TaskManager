
using TaskManager.API.Task.DTOs;
using TaskManager.API.Task.Models;
namespace TaskManager.API.Task
{
    public interface ITaskRepository
    {
        Task<List<TaskModel>> GetAllTasksAsync();
        Task<TaskModel> GetTaskByIdAsync(int id);
        Task<TaskModel> CreateTaskAsync(TaskModel taskDto);
        // Task<bool> UpdateTaskAsync(int id, TaskUpdateDto taskDto);
        // Task<bool> DeleteTaskAsync(int id);
        // Task<List<TaskResponseDto>> GetCurrentUserTasksAsync();
    }
}