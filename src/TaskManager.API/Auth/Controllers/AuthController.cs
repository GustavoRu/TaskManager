using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TaskManager.API.Auth.DTOs;
using TaskManager.API.User.Models;
using TaskManager.API.Shared.Data;
using TaskManager.API.Shared.Models;
using TaskManager.API.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
namespace TaskManager.API.Auth.Controllers
{

    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AuthController : ControllerBase
    {

        // public AuthController()
        // {

        // }
    }
}