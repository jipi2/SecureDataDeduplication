using FileStorageApp.Server.Services;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;

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
        public async Task<IActionResult> Register(RegisterUser regUser)
        {
            try
            {
                return Ok(await _userService.Register(regUser));
            }
            catch (ExceptionModel e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
            try
            {
                Response resp = await _userService.Login(loginUser);
                return Ok(resp);
            }
            catch (ExceptionModel e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("test")]
        [Authorize(Roles = "client")]
        public IActionResult GetTestClient()
        {
            return Ok("Merge frate");
        }

        [HttpPost("testDto")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetTestClientDto([FromBody] TestDto testDto)
        {
            return Ok(testDto);
        }

        [HttpPost("CreateUser")]
        [Authorize (Roles = "admin")]
        public async Task<IActionResult> AddUser([FromBody] RegisterUser regUser)
        {
            try
            {
                return Ok(await _userService.Register(regUser));
            }
            catch (ExceptionModel e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("AddProxy")]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> AddProxy([FromBody] RegisterProxyDto regProxy)
        {
            try
            {
                return Ok(await _userService.AddProxy(regProxy));
            }
            catch(ExceptionModel e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet ("testProxyController")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> testProxyController()
        {
            return Ok("Succes!");
        }

        [HttpPost("GetUserEmail")]
        [Authorize (Roles ="proxy")]
        public async Task<IActionResult> GetUserEmail([FromBody] string userJWT)
        {
            string id = await _userService.GetUserIdFromJWT(userJWT);
            return Ok(await _userService.GetUserEmail(id));
        }
    }
}
