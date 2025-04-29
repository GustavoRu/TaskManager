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
        [Fact]
        public async System.Threading.Tasks.Task GetAllTasksAsync_ReturnsListOfTaskResponseDto()
        {
            // Arrange
            var fakeTasks = new List<TaskModel>
            {
                new TaskModel { TaskId = 1, Title = "Test Task", Description = "Description", IsCompleted = false, OwnerId = 1, CreatedAt = System.DateTime.UtcNow }
            };

            var mockRepo = new Mock<ITaskRepository>();
            mockRepo.Setup(repo => repo.GetAllTasksAsync()).ReturnsAsync(fakeTasks);

            var mockUserContext = new Mock<IUserContextAccessor>();

            var service = new TaskService(mockRepo.Object, mockUserContext.Object);

            // Act
            var result = await service.GetAllTasksAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Test Task");
        }
    }
}
