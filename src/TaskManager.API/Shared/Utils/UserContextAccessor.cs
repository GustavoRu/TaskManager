using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TaskManager.API.Shared.Utils
{
    public interface IUserContextAccessor
    {
        int GetCurrentUserId();
    }

    public class UserContextAccessor : IUserContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Usuario no válido o no autenticado");
            }

            return userId;
        }
    }
}