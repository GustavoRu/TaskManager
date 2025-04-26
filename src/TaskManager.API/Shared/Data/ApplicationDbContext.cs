using Microsoft.EntityFrameworkCore;
using TaskManager.API.Shared.Models;
using TaskManager.API.Task.Models;
using TaskManager.API.User;
namespace TaskManager.API.Shared.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        // public DbSet<TaskCollaborationModel> TaskCollaborations { get; set; }
    }
}