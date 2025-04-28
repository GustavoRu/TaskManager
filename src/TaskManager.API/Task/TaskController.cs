using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Shared.Data;
using TaskManager.API.Task.Models;
using TaskManager.API.Task.DTOs;


namespace TaskManager.API.Task
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITaskService _taskService;

        public TaskController(ApplicationDbContext dbContext, ITaskService taskService)
        {
            _dbContext = dbContext;
            _taskService = taskService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Usuario no válido o no autenticado");
            }

            return userId;
        }


        [HttpGet]
        [Route("getall")]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            return Ok(task);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] TaskPostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();

            var task = new TaskModel
            {
                Title = dto.Title,
                Description = dto.Description,
                OwnerId = userId,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            var historyEntry = new TaskHistoryModel
            {
                TaskId = task.TaskId,
                UserId = userId,
                Action = TaskAction.Created,
                Timestamp = DateTime.UtcNow,
                ChangesJson = System.Text.Json.JsonSerializer.Serialize(new { })
            };

            await _dbContext.TaskHistories.AddAsync(historyEntry);
            await _dbContext.SaveChangesAsync();

            var response = new TaskResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                OwnerId = task.OwnerId
            };

            return CreatedAtAction(nameof(GetById), new { id = task.TaskId }, response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = await _dbContext.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();


            task.Title = dto.Title;
            task.Description = dto.Description;
            task.IsCompleted = dto.IsCompleted;
            // task.Priority = dto.Priority;
            // task.DueDate = dto.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var userId = GetCurrentUserId();
            var historyEntry = new TaskHistoryModel
            {
                TaskId = task.TaskId,
                UserId = userId,
                Action = TaskAction.Updated,
                ChangesJson = System.Text.Json.JsonSerializer.Serialize(new { dto.Title, dto.Description, dto.IsCompleted }),
                Timestamp = DateTime.UtcNow
            };
            await _dbContext.TaskHistories.AddAsync(historyEntry);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("mytasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            var userId = GetCurrentUserId();

            var tasks = await _dbContext.Tasks
                .Where(t => t.OwnerId == userId)
                .Include(t => t.Owner)
                .ToListAsync();

            var taskDtos = tasks.Select(t => new TaskResponseDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                OwnerId = t.OwnerId
            }).ToList();

            return Ok(taskDtos);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _dbContext.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();

            var userId = GetCurrentUserId();
            var historyEntry = new TaskHistoryModel
            {
                TaskId = id,
                UserId = userId,
                Action = TaskAction.Deleted,
                Timestamp = DateTime.UtcNow
            };
            await _dbContext.TaskHistories.AddAsync(historyEntry);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

    }
}