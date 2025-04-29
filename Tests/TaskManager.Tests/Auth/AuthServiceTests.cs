using Xunit;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.API.Auth;
using TaskManager.API.Auth.DTOs;
using TaskManager.API.Shared.Utils;
using TaskManager.API.User;

namespace TaskManager.Tests.Auth
{
    public class AuthServiceTests
    {
        private Mock<IAuthRepository> _mockAuthRepository;
        private Mock<IJwtUtility> _mockJwtUtility;
        private AuthService _authService;

        public AuthServiceTests()
        {
            _mockAuthRepository = new Mock<IAuthRepository>();
            _mockJwtUtility = new Mock<IJwtUtility>();
            _authService = new AuthService(_mockAuthRepository.Object, _mockJwtUtility.Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task RegisterUserAsync_WhenUserCreatedSuccessfully_ReturnsTrue()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "password123"
            };

            var passwordHash = "hashed_password";
            _mockJwtUtility.Setup(x => x.encryptSHA256(registerDto.Password)).Returns(passwordHash);
            _mockAuthRepository.Setup(x => x.CreateUserAsync(It.IsAny<UserModel>()))
                .ReturnsAsync(new UserModel { UserId = 1, Name = registerDto.Name, Email = registerDto.Email });

            // Act
            var result = await _authService.RegisterUserAsync(registerDto);

            // Assert
            result.Should().BeTrue();
            _mockAuthRepository.Verify(x => x.CreateUserAsync(It.Is<UserModel>(u => 
                u.Name == registerDto.Name && 
                u.Email == registerDto.Email && 
                u.PasswordHash == passwordHash)), Times.Once);
        }
    }
}