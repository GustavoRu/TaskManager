namespace TaskManager.API.Task.DTOs
{
    public class TaskUpdateDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        // public int Priority { get; set; }
        // public DateTime? DueDate { get; set; }
    }
}
