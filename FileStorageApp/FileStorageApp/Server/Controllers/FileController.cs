using FileStorageApp.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using System.Net.Http.Headers;

namespace FileStorageApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        FileService _fileService { get; set; }
        UserService _userService { get; set; }
        public FileController(FileService fileService, UserService userService)
        {
            _fileService = fileService;
            _userService = userService;
        }

        [HttpPost("upload")]
        [Authorize(Roles = "client ")]
        public async Task<IActionResult> Upload(IFormFile file)
        {

            return Ok("Merge frate, are atata lungime: " + file.Length.ToString());
        }

        [HttpGet("testFile")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetId()
        {
            string token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            string id = await _userService.GetUserIdFromJWT(token);
            return Ok(id);
        }

        [HttpGet("dfParameters")]
        [Authorize(Roles = "client")]
        public async Task<Dictionary<string, string>> GetDFParameters()
        {
            string token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            string id = await _userService.GetUserIdFromJWT(token);
            Dictionary<string, string> parameters = await _fileService.GetDFParameters(id);

            return parameters;
        }

        [HttpPost("DFexchange")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> DFkeyExchange([FromBody] string pubKey)
        {
            string token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            string id = await _userService.GetUserIdFromJWT(token);
            if (await _fileService.DFkeyExchange(pubKey, id))
                return Ok("Succes");
            else
                return BadRequest("Fail");
        }
    }
}
