using TaskManager.API.Task.DTOs;
namespace TaskManager.API.Task
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetAllTasksAsync();
        Task<TaskResponseDto> GetTaskByIdAsync(int id);
        // Task<TaskResponseDto> CreateTaskAsync(TaskPostDto taskDto);
        // Task<bool> UpdateTaskAsync(int id, TaskUpdateDto taskDto);
        // Task<bool> DeleteTaskAsync(int id);
        // Task<List<TaskResponseDto>> GetCurrentUserTasksAsync();
    }
}