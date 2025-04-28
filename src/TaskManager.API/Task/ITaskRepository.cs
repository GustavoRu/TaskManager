
using TaskManager.API.Task.DTOs;
using TaskManager.API.Task.Models;
namespace TaskManager.API.Task
{
    public interface ITaskRepository
    {
        Task<List<TaskModel>> GetAllTasksAsync();
        Task<TaskModel> GetTaskByIdAsync(int id);
        Task<TaskModel> CreateTaskAsync(TaskModel task);
        Task<bool> UpdateTaskAsync(TaskModel task);
         Task<List<TaskModel>> GetTasksByUserIdAsync(int userId);
        // Task<bool> DeleteTaskAsync(int id);
        // Task<List<TaskResponseDto>> GetCurrentUserTasksAsync();

        System.Threading.Tasks.Task AddTaskHistoryAsync(TaskHistoryModel history);
    }
}