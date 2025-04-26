using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TaskManager.API.Auth.DTOs;
using TaskManager.API.User;
using TaskManager.API.Shared.Data;
using TaskManager.API.Shared.Models;
using TaskManager.API.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace TaskManager.API.Auth
{

    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtUtility _jwtUtility;

        public AuthController(ApplicationDbContext context, JwtUtility jwtUtility)
        {
            _context = context;
            _jwtUtility = jwtUtility;
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var user = new UserModel
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = _jwtUtility.encryptSHA256(registerDto.Password)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            if (user.UserId != 0)
            {
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true });
            }

            return StatusCode(StatusCodes.Status200OK, new { isSuccess = false });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var userFind = await _context.Users.Where(u => u.Email == loginDto.Email && u.PasswordHash == _jwtUtility.encryptSHA256(loginDto.Password)).FirstOrDefaultAsync();

            if (userFind == null)
            {
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = false });
            }
            return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, token = _jwtUtility.GenerateJwtToken(userFind) });

        }
    }

}