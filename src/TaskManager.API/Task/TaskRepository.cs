
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.API.Shared.Data;
using TaskManager.API.Task;
using TaskManager.API.Task.Models;
namespace TaskManager.API.Task
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public TaskRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<TaskModel>> GetAllTasksAsync()
        {
            return await _dbContext.Tasks
                .Include(t => t.Owner)
                .ToListAsync();
        }

        public async Task<TaskModel> GetTaskByIdAsync(int id)
        {
            return await _dbContext.Tasks
                .Include(t => t.Owner)
                .FirstOrDefaultAsync(t => t.TaskId == id);
        }

        public async Task<TaskModel> CreateTaskAsync(TaskModel task)
        {
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();
            return task;
        }

        public async Task<bool> UpdateTaskAsync(TaskModel task)
        {
            _dbContext.Tasks.Update(task);
            var affectedRows = await _dbContext.SaveChangesAsync();
            return affectedRows > 0;
        }




        public async System.Threading.Tasks.Task AddTaskHistoryAsync(TaskHistoryModel historyEntry)
        {
            await _dbContext.TaskHistories.AddAsync(historyEntry);
            await _dbContext.SaveChangesAsync();
        }

    }
}