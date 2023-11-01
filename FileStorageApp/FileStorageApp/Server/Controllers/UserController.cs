using FileStorageApp.Server.Services;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<Response> Register(RegisterUser regUser)
        {
            return await _userService.Register(regUser);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<Response> Login(LoginUser loginUser)
        {
            return await _userService.Login(loginUser);
        }


        [HttpGet("test")]
        [Authorize(Roles = "client")]
        public IActionResult GetTestClient()
        {
            return Ok("Merge frate");
        }

    }
}
