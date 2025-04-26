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
        public DbSet<TaskHistoryModel> TaskHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación TaskHistory → User
            modelBuilder.Entity<TaskHistoryModel>()
                .HasOne(h => h.User)
                .WithMany() // No necesito la colección inversa
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.NoAction); // No Cascade

            // Relación TaskHistory → Task
            modelBuilder.Entity<TaskHistoryModel>()
                .HasOne(h => h.Task)
                .WithMany(t => t.History) // Desde Task puedo ver su historial
                .HasForeignKey(h => h.TaskId)
                .OnDelete(DeleteBehavior.Cascade); // Si borro la tarea, borro su historial
        }

    }
}