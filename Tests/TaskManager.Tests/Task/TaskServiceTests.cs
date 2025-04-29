using Xunit;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.API.Task;
using TaskManager.API.Task.Models;
using TaskManager.API.Task.DTOs;
using TaskManager.API.Shared.Utils;

namespace TaskManager.Tests.Task
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _mockRepo;
        private readonly Mock<IUserContextAccessor> _mockUserContext;
        private readonly TaskService _service;
        private readonly int _userId = 123;

        public TaskServiceTests()
        {
            _mockRepo = new Mock<ITaskRepository>();
            _mockUserContext = new Mock<IUserContextAccessor>();
            _mockUserContext.Setup(u => u.GetCurrentUserId()).Returns(_userId);
            _service = new TaskService(_mockRepo.Object, _mockUserContext.Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAllTasksAsync_ReturnsListOfTaskResponseDto()
        {
            // Arrange
            var fakeTasks = new List<TaskModel>
            {
                new TaskModel { TaskId = 1, Title = "Test Task", Description = "Description", IsCompleted = false, OwnerId = 1, CreatedAt = DateTime.UtcNow }
            };

            _mockRepo.Setup(repo => repo.GetAllTasksAsync()).ReturnsAsync(fakeTasks);

            // Act
            var result = await _service.GetAllTasksAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Test Task");
            result[0].TaskId.Should().Be(1);
            result[0].Description.Should().Be("Description");
            result[0].IsCompleted.Should().BeFalse();
            result[0].OwnerId.Should().Be(1);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAllTasksAsync_ReturnsEmptyList_WhenNoTasksExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetAllTasksAsync()).ReturnsAsync(new List<TaskModel>());

            // Act
            var result = await _service.GetAllTasksAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskByIdAsync_ReturnsTask_WhenTaskExists()
        {
            // Arrange
            var fakeTask = new TaskModel
            {
                TaskId = 1,
                Title = "Test Task",
                Description = "Description",
                IsCompleted = false,
                OwnerId = 1,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepo.Setup(r => r.GetTaskByIdAsync(1)).ReturnsAsync(fakeTask);

            // Act
            var result = await _service.GetTaskByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.TaskId.Should().Be(1);
            result.Title.Should().Be("Test Task");
            result.Description.Should().Be("Description");
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskByIdAsync_ReturnsNull_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetTaskByIdAsync(999)).ReturnsAsync((TaskModel)null);

            // Act
            var result = await _service.GetTaskByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskAsync_ReturnsCreatedTaskResponseDto()
        {
            // Arrange
            var taskPostDto = new TaskPostDto
            {
                Title = "New Task",
                Description = "New Description"
            };

            var taskModelCreated = new TaskModel
            {
                TaskId = 99,
                Title = taskPostDto.Title,
                Description = taskPostDto.Description,
                OwnerId = _userId,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepo.Setup(r => r.CreateTaskAsync(It.IsAny<TaskModel>())).ReturnsAsync(taskModelCreated);
            _mockRepo.Setup(r => r.AddTaskHistoryAsync(It.IsAny<TaskHistoryModel>())).Returns(System.Threading.Tasks.Task.CompletedTask);

            // Act
            var result = await _service.CreateTaskAsync(taskPostDto);

            // Assert
            result.Should().NotBeNull();
            result.TaskId.Should().Be(99);
            result.Title.Should().Be("New Task");
            result.OwnerId.Should().Be(_userId);
            result.IsCompleted.Should().BeFalse();

            // Verificar que se llama a AddTaskHistoryAsync
            _mockRepo.Verify(r => r.AddTaskHistoryAsync(
                It.Is<TaskHistoryModel>(h => h.TaskId == 99 && h.UserId == _userId && h.Action == TaskAction.Created)
            ), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskAsync_ReturnsTrue_WhenTaskExists()
        {
            // Arrange
            int taskId = 1;
            var existingTask = new TaskModel
            {
                TaskId = taskId,
                Title = "Original Title",
                Description = "Original Description",
                IsCompleted = false,
                OwnerId = _userId,
                CreatedAt = DateTime.UtcNow
            };

            var updateDto = new TaskUpdateDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                IsCompleted = true
            };

            _mockRepo.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(existingTask);
            _mockRepo.Setup(r => r.UpdateTaskAsync(It.IsAny<TaskModel>())).ReturnsAsync(true);
            _mockRepo.Setup(r => r.AddTaskHistoryAsync(It.IsAny<TaskHistoryModel>())).Returns(System.Threading.Tasks.Task.CompletedTask);

            // Act
            var result = await _service.UpdateTaskAsync(taskId, updateDto);

            // Assert
            result.Should().BeTrue();
            
            // Verificar que se actualizó el task con los valores correctos
            _mockRepo.Verify(r => r.UpdateTaskAsync(It.Is<TaskModel>(
                t => t.TaskId == taskId &&
                t.Title == "Updated Title" &&
                t.Description == "Updated Description" &&
                t.IsCompleted == true &&
                t.UpdatedAt != null
            )), Times.Once);

            // Verificar que se registró la historia del cambio
            _mockRepo.Verify(r => r.AddTaskHistoryAsync(
                It.Is<TaskHistoryModel>(h => 
                    h.TaskId == taskId && 
                    h.UserId == _userId && 
                    h.Action == TaskAction.Updated &&
                    h.ChangesJson.Contains("Updated Title") &&
                    h.ChangesJson.Contains("Updated Description")
                )
            ), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskAsync_ReturnsFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            int taskId = 999;
            var updateDto = new TaskUpdateDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                IsCompleted = true
            };

            _mockRepo.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync((TaskModel)null);

            // Act
            var result = await _service.UpdateTaskAsync(taskId, updateDto);

            // Assert
            result.Should().BeFalse();
            
            // Verificar que no se intentó actualizar ni registrar historia
            _mockRepo.Verify(r => r.UpdateTaskAsync(It.IsAny<TaskModel>()), Times.Never);
            _mockRepo.Verify(r => r.AddTaskHistoryAsync(It.IsAny<TaskHistoryModel>()), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetCurrentUserTasksAsync_ReturnsUserTasks()
        {
            // Arrange
            var userTasks = new List<TaskModel>
            {
                new TaskModel { TaskId = 1, Title = "User Task 1", OwnerId = _userId },
                new TaskModel { TaskId = 2, Title = "User Task 2", OwnerId = _userId }
            };

            _mockRepo.Setup(r => r.GetTasksByUserIdAsync(_userId)).ReturnsAsync(userTasks);

            // Act
            var result = await _service.GetCurrentUserTasksAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(t => t.OwnerId == _userId).Should().BeTrue();
            result[0].Title.Should().Be("User Task 1");
            result[1].Title.Should().Be("User Task 2");
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteTaskAsync_ReturnsTrue_WhenTaskIsDeleted()
        {
            // Arrange
            int taskId = 1;
            _mockRepo.Setup(r => r.DeleteTaskAsync(taskId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteTaskAsync(taskId);

            // Assert
            result.Should().BeTrue();
            _mockRepo.Verify(r => r.DeleteTaskAsync(taskId), Times.Once);
            
            _mockRepo.Verify(r => r.AddTaskHistoryAsync(It.IsAny<TaskHistoryModel>()), Times.Never);
        }
    
        [Fact]
        public async System.Threading.Tasks.Task DeleteTaskAsync_ReturnsFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            int taskId = 999;
            _mockRepo.Setup(r => r.DeleteTaskAsync(taskId)).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteTaskAsync(taskId);

            // Assert
            result.Should().BeFalse();
            _mockRepo.Verify(r => r.DeleteTaskAsync(taskId), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskHistoryEntryAsync_CallsAddTaskHistoryAsync()
        {
            // Arrange
            int taskId = 1;
            var changes = new { Field = "Value" };
            
            _mockRepo.Setup(r => r.AddTaskHistoryAsync(It.IsAny<TaskHistoryModel>())).Returns(System.Threading.Tasks.Task.CompletedTask);

            // Act
            await _service.CreateTaskHistoryEntryAsync(taskId, _userId, TaskAction.Updated, changes);

            // Assert
            _mockRepo.Verify(r => r.AddTaskHistoryAsync(It.Is<TaskHistoryModel>(
                h => h.TaskId == taskId && 
                     h.UserId == _userId && 
                     h.Action == TaskAction.Updated &&
                     h.ChangesJson.Contains("Field") &&
                     h.ChangesJson.Contains("Value")
            )), Times.Once);
        }

    }
}
