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

            var createdTask = await _taskService.CreateTaskAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdTask.TaskId }, createdTask);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.UpdateTaskAsync(id, dto);
            if (!result)
                return NotFound();
            
            return NoContent();
        }

        [HttpGet("mytasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            var tasks = await _taskService.GetCurrentUserTasksAsync();
            return Ok(tasks);
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