using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManager.API.Task.Models;
// using TaskManager.API.User.Models;
namespace TaskManager.API.Shared.Models
{
    public class TaskCollaborationModel
    {
        // [Key]
        // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // public int CollaborationId { get; set; }

        // public int TaskId { get; set; }
        // [ForeignKey("TaskId")]
        // public TaskModel? Task { get; set; }

        // public int UserId { get; set; }
        // [ForeignKey("UserId")]
        // public User.Models.UserModel? User { get; set; }

        // public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // // Opcional: permisos del colaborador (lectura, edición, etc.)
        // public int PermissionLevel { get; set; } = 1; // 1=Read, 2=Edit, etc.
    }
}