using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManager.API.Shared.Models;
namespace TaskManager.API.Task.Models
{
    public class TaskModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; } = 0; // 0=Normal, 1=High, etc.

        // FK y navegación al propietario
        public int OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public User.Models.UserModel? Owner { get; set; }

        // Colaboradores
        // public ICollection<TaskCollaborationModel>? Collaborators { get; set; }
    }
}
