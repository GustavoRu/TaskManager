using TaskManager.API.User;
namespace TaskManager.API.Auth
{
    public interface IAuthRepository
    {
        Task<UserModel> CreateUserAsync(UserModel user);
        Task<UserModel> GetUserByEmailAndPasswordHashAsync(string email, string passwordHash);
    }
}