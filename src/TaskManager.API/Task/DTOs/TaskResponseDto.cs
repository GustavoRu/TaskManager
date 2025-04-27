namespace TaskManager.API.Task.DTOs
{
    public class TaskResponseDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OwnerId { get; set; }
    }

}