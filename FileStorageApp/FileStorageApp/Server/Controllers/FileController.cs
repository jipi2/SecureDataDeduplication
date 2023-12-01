using FileStorageApp.Server.Services;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
            if (token.IsNullOrEmpty())
            {
                return BadRequest("Invalid toke");
            }
            string id = await _userService.GetUserIdFromJWT(token);
            return Ok(id);
        }

        [HttpGet("dfParameters")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetDFParameters()
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return null;
                }
                string id = await _userService.GetUserIdFromJWT(token);
                DFparametersDto parameters = await _fileService.GetDFParameters(id);

                return Ok(parameters);
            }
            catch (Exception e)
            {
                return BadRequest("Server Error");
            }
        }

        [HttpPost("DFexchange")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> DFkeyExchange([FromBody] string pubKey)
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return BadRequest("Invalid toke");
            }
            string id = await _userService.GetUserIdFromJWT(token);
            if (await _fileService.DFkeyExchange(pubKey, id))
                return Ok("Succes");
            else
                return BadRequest("Fail");
        }

        [HttpPost("uploadFile")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetTagAndEncFile([FromBody] FileParamsDto fileParams)
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return BadRequest("Invalid toke");
            }
            string id = await _userService.GetUserIdFromJWT(token);
            FileMetaChallenge result = await _fileService.ComputeFileMetadata(fileParams, id);
            
            if(result == null)
            {
                return BadRequest("Error");
            }

            if(result.id == "")
                return Ok(result);
            if(result.id == "File name already exists!")
                return BadRequest("A file with this name already exists!");
            else if (result.id != "")
            {   
                return Ok(result);
            }
            return BadRequest("Error");
        }

        [HttpGet("getUploadedFileNamesAndDates")]
        [Authorize(Roles = "client")]
        public async Task<List<FilesNameDate>?> GetFileNamesAndDates()
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return null;
            }
            string id = await _userService.GetUserIdFromJWT(token);
            List<FilesNameDate>? result = await _fileService.GetFileNamesAndDatesOfUser(id);
            
            return result;
        }

        [HttpPost("getFileFromStorage")]
        [Authorize(Roles = "client")]
        public async Task<ServerBlobFIle> GetFileFromStorage([FromBody] string fileName)
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return null;
            }
            string id = await _userService.GetUserIdFromJWT(token);
            ServerBlobFIle severFile = await _fileService.GetFileFromBlob(id, fileName);
            return severFile;
        }

        [HttpPost("verifyFileChallenge")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetChallengeResponse([FromBody] FileResp fr)
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return BadRequest("Invalid toke");
            }
            string id = await _userService.GetUserIdFromJWT(token);
            bool result = await _fileService.SaveFileToUser(id, fr);
            if (result == false)
                return BadRequest("The answer for challenge was wrong!");
            return Ok("You're file has been uploaded");
        }
    }
}
