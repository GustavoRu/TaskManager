using TaskManager.API.Auth.DTOs;
using TaskManager.API.User;
using TaskManager.API.Shared.Models;
using TaskManager.API.Shared.Utils;
namespace TaskManager.API.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtUtility _jwtUtility;
        public List<string> Errors { get; }

        public AuthService(IAuthRepository authRepository, IJwtUtility jwtUtility)
        {
            _authRepository = authRepository;
            _jwtUtility = jwtUtility;
            Errors = new List<string>();
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

        public bool Validate(RegisterDto registerDto)
        {
            if (_authRepository.Search(u => u.Email == registerDto.Email).Count() > 0)
            {
                Errors.Add("El correo ya existe");
                return false;
            }
            return true;
        }
    }
}