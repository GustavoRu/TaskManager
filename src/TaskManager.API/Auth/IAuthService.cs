using TaskManager.API.Auth.DTOs;
namespace TaskManager.API.Auth
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(RegisterDto registerDto);
        bool Validate(RegisterDto registerDto);
        Task<(bool isSuccess, string token, int userId)> LoginUserAsync(LoginDto loginDto);
        public List<string> Errors { get; }
    }
}