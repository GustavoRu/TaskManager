using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Shared.Data;
using TaskManager.API.Task.DTOs;
using TaskManager.API.Task.Validators;

namespace TaskManager.API.Task
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly TaskPostDtoValidator _taskPostValidator;
        private readonly TaskUpdateDtoValidator _taskUpdateDtoValidator;
        public TaskController(ITaskService taskService, TaskPostDtoValidator taskPostValidator, TaskUpdateDtoValidator taskUpdateDtoValidator)
        {
            _taskService = taskService;
            _taskPostValidator = taskPostValidator;
            _taskUpdateDtoValidator = taskUpdateDtoValidator;
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
            var validationResult = await _taskPostValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var createdTask = await _taskService.CreateTaskAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdTask.TaskId }, createdTask);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto dto)
        {
            var validationResult = await _taskUpdateDtoValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

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
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

    }
}