using TaskManager.API.User;

namespace TaskManager.API.Task.Models
{
    public class TaskHistoryModel
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public TaskModel Task { get; set; } = null!;

        public int UserId { get; set; }
        public UserModel User { get; set; } = null!;

        public TaskAction Action { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Opcional: JSON con los valores antes/después
        public string? ChangesJson { get; set; }
    }
    public enum TaskAction
    {
        Created,
        Updated,
        Deleted,
        Completed,
        Reopened
    }
}
