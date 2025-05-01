using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TaskManager.API.Auth.DTOs;
using TaskManager.API.Task.DTOs;
using TaskManager.API.Task.Models;
using TaskManager.API.User;
using Xunit;
using FluentAssertions;

namespace TaskManager.Tests.Task
{
    public class TaskControllerIntegrationTests : IntegrationTestBase
    {
        private string _authToken;
        private int _userId;

        public TaskControllerIntegrationTests()
        {
            SetupTestUser().Wait();
        }

        private async System.Threading.Tasks.Task SetupTestUser()
        {
            try
            {
                var registerDto = new RegisterDto
                {
                    Name = "Test User",
                    Email = "test@example.com",
                    Password = "Test123!"
                };

                // Register user
                var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);
                var registerContent = await registerResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Register Response Status: {registerResponse.StatusCode}");
                Console.WriteLine($"Register Response Content: {registerContent}");
                
                if (!registerResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to register user. Status: {registerResponse.StatusCode}, Content: {registerContent}");
                }

                // Login to get token
                var loginDto = new LoginDto
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password
                };

                var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
                var loginContent = await loginResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Login Response Status: {loginResponse.StatusCode}");
                Console.WriteLine($"Login Response Content: {loginContent}");

                if (!loginResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to login. Status: {loginResponse.StatusCode}, Content: {loginContent}");
                }

                var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (loginResult == null || string.IsNullOrEmpty(loginResult.Token))
                {
                    throw new Exception("Login response did not contain a valid token");
                }

                _authToken = loginResult.Token;
                _userId = loginResult.UserId;

                // Set token for subsequent requests
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SetupTestUser: {ex}");
                throw;
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAll_ReturnsEmptyList_WhenNoTasksExist()
        {
            // Act
            var response = await _client.GetAsync("/api/Task/getall");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
            tasks.Should().NotBeNull();
            tasks.Should().BeEmpty();
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_ReturnsCreatedTask_WhenValidInput()
        {
            // Arrange
            var taskDto = new TaskPostDto
            {
                Title = "Test Task",
                Description = "Test Description",
                IsCompleted = false
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Task/create", taskDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdTask = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
            createdTask.Should().NotBeNull();
            createdTask.Title.Should().Be(taskDto.Title);
            createdTask.Description.Should().Be(taskDto.Description);
            createdTask.IsCompleted.Should().Be(taskDto.IsCompleted);
            createdTask.OwnerId.Should().Be(_userId);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetById_ReturnsTask_WhenTaskExists()
        {
            // Arrange
            var taskDto = new TaskPostDto
            {
                Title = "Test Task",
                Description = "Test Description",
                IsCompleted = false
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Task/create", taskDto);
            var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

            // Act
            var response = await _client.GetAsync($"/api/Task/getbyid/{createdTask.TaskId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var task = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
            task.Should().NotBeNull();
            task.TaskId.Should().Be(createdTask.TaskId);
            task.Title.Should().Be(taskDto.Title);
            task.Description.Should().Be(taskDto.Description);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetById_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/Task/getbyid/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async System.Threading.Tasks.Task Update_ReturnsNoContent_WhenTaskUpdatedSuccessfully()
        {
            // Arrange
            var taskDto = new TaskPostDto
            {
                Title = "Original Title",
                Description = "Original Description"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Task/create", taskDto);
            var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

            var updateDto = new TaskUpdateDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                IsCompleted = true
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/Task/update/{createdTask.TaskId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/Task/getbyid/{createdTask.TaskId}");
            var updatedTask = await getResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
            updatedTask.Should().NotBeNull();
            updatedTask.Title.Should().Be(updateDto.Title);
            updatedTask.Description.Should().Be(updateDto.Description);
            updatedTask.IsCompleted.Should().Be(updateDto.IsCompleted);
        }

        [Fact]
        public async System.Threading.Tasks.Task Update_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var updateDto = new TaskUpdateDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                IsCompleted = true
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/Task/update/999", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async System.Threading.Tasks.Task Delete_ReturnsNoContent_WhenTaskDeletedSuccessfully()
        {
            // Arrange
            var taskDto = new TaskPostDto
            {
                Title = "Test Task",
                Description = "Test Description"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Task/create", taskDto);
            var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/Task/delete/{createdTask.TaskId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the task was deleted
            var getResponse = await _client.GetAsync($"/api/Task/getbyid/{createdTask.TaskId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async System.Threading.Tasks.Task Delete_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Act
            var response = await _client.DeleteAsync("/api/Task/delete/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetMyTasks_ReturnsOnlyUserOwnedTasks()
        {
            // Arrange
            var task1 = new TaskPostDto { Title = "Task 1", Description = "Description 1" };
            var task2 = new TaskPostDto { Title = "Task 2", Description = "Description 2" };

            await _client.PostAsJsonAsync("/api/Task/create", task1);
            await _client.PostAsJsonAsync("/api/Task/create", task2);

            // Act
            var response = await _client.GetAsync("/api/Task/mytasks");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(2);
            tasks.Should().AllSatisfy(t => t.OwnerId.Should().Be(_userId));
        }
    }

    public class LoginResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
    }
}
