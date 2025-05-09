using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.Auth.DTOs;
namespace TaskManager.API.Auth
{

    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly RegisterDtoValidator _registerValidator;
        private readonly LoginDtoValidator _loginValidator;

        public AuthController(IAuthService authService,
            RegisterDtoValidator registerValidator,
            LoginDtoValidator loginValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { isSuccess = false, errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList() });
            }
            if (!_authService.Validate(registerDto))
            {
                return BadRequest(new { isSuccess = false, errors = _authService.Errors });
            }

            var isSuccess = await _authService.RegisterUserAsync(registerDto);
            return Ok(new { isSuccess });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var validationResult = await _loginValidator.ValidateAsync(loginDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { isSuccess = false, errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList() });
            }
            var (isSuccess, token, userId, userName) = await _authService.LoginUserAsync(loginDto);
            return Ok(new { isSuccess, token, userId, userName });
        }
    }

}