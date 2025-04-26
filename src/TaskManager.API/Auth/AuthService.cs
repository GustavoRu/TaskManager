using System.Threading.Tasks;
using TaskManager.API.Auth.DTOs;
using TaskManager.API.Auth;
using TaskManager.API.User;
using TaskManager.API.Shared.Models;
using TaskManager.API.Shared.Utils;
namespace TaskManager.API.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly JwtUtility _jwtUtility;

        public AuthService(IAuthRepository authRepository, JwtUtility jwtUtility)
        {
            _authRepository = authRepository;
            _jwtUtility = jwtUtility;
        }

        public async Task<bool> RegisterUserAsync(RegisterDto registerDto)
        {
            var user = new UserModel
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = _jwtUtility.encryptSHA256(registerDto.Password)
            };
            var createdUser = await _authRepository.CreateUserAsync(user);
            if (createdUser != null)
            {
                return true;
            }
            return false;
        }

        public async Task<(bool isSuccess, string token, int userId)> LoginUserAsync(LoginDto loginDto)
        {
            var passwordHash = _jwtUtility.encryptSHA256(loginDto.Password);
            var user = await _authRepository.GetUserByEmailAndPasswordHashAsync(loginDto.Email, passwordHash);

            if (user == null)
            {
                return (false, null, 0);
            }

            var token = _jwtUtility.GenerateJwtToken(user);
            return (true, token, user.UserId);
        }

    }
}