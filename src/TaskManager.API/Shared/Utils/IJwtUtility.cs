namespace TaskManager.API.Shared.Utils
{
    public interface IJwtUtility
    {
        string encryptSHA256(string text);
        string GenerateJwtToken(User.UserModel user);
    }
}