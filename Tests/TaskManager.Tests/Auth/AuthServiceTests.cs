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
            // arrange
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

            // sct
            var result = await _authService.RegisterUserAsync(registerDto);

            // Assert
            result.Should().BeTrue();
            _mockAuthRepository.Verify(x => x.CreateUserAsync(It.Is<UserModel>(u =>
                u.Name == registerDto.Name &&
                u.Email == registerDto.Email &&
                u.PasswordHash == passwordHash)), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task RegisterUserAsync_WhenUserCreationFails_ReturnsFalse()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "password123"
            };

            _mockJwtUtility.Setup(x => x.encryptSHA256(registerDto.Password)).Returns("hashed_password");
            _mockAuthRepository.Setup(x => x.CreateUserAsync(It.IsAny<UserModel>())).ReturnsAsync((UserModel)null);

            // Act
            var result = await _authService.RegisterUserAsync(registerDto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async System.Threading.Tasks.Task LoginUserAsync_WithValidCredentials_ReturnsSuccessWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var passwordHash = "hashed_password";
            var user = new UserModel { UserId = 1, Name = "Test User", Email = loginDto.Email, PasswordHash = passwordHash };
            var token = "jwt_token";

            _mockJwtUtility.Setup(x => x.encryptSHA256(loginDto.Password)).Returns(passwordHash);
            _mockAuthRepository.Setup(x => x.GetUserByEmailAndPasswordHashAsync(loginDto.Email, passwordHash))
                .ReturnsAsync(user);
            _mockJwtUtility.Setup(x => x.GenerateJwtToken(user)).Returns(token);

            // Act
            var result = await _authService.LoginUserAsync(loginDto);

            // Assert
            result.isSuccess.Should().BeTrue();
            result.token.Should().Be(token);
            result.userId.Should().Be(user.UserId);
            result.userName.Should().Be(user.Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task LoginUserAsync_WithInvalidCredentials_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };
            var passwordHash = "hashed_password";
            _mockJwtUtility.Setup(x => x.encryptSHA256(loginDto.Password)).Returns(passwordHash);
            _mockAuthRepository.Setup(x => x.GetUserByEmailAndPasswordHashAsync(loginDto.Email, passwordHash))
                .ReturnsAsync((UserModel)null);

            // act
            var result = await _authService.LoginUserAsync(loginDto);

            //assert
            result.isSuccess.Should().BeFalse();
            result.token.Should().BeNull();
            result.userId.Should().Be(0);
            result.userName.Should().BeNull();
        }

        [Fact]
        public void Validate_WhenEmailExists_ReturnsFalseAndAddsError()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Test User",
                Email = "existing@example.com",
                Password = "password123"
            };

            var existingUsers = new List<UserModel>
            {
                new UserModel { UserId = 1, Email = registerDto.Email }
            };

            _mockAuthRepository.Setup(x => x.Search(It.IsAny<Func<UserModel, bool>>()))
                .Returns(existingUsers.AsQueryable());

            // Act
            var result = _authService.Validate(registerDto);

            // assert
            result.Should().BeFalse();
            _authService.Errors.Should().ContainSingle().And.Contain("El correo ya existe");
        }
    }
}