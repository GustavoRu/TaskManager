namespace TaskManager.API.Task.DTOs
{
    public class TaskPostDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int UserId { get; set; }
        public bool IsCompleted { get; set; } = false;


    }
}