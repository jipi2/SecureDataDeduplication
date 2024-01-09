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

        [HttpPost("checkEncTag")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> CheckEncTag([FromBody] TagDto encTag)
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return BadRequest("Invalid toke");
            }
            string id = await _userService.GetUserIdFromJWT(token);
            bool tagExists = false;
            try
            {
                tagExists = await _fileService.CheckEncTag(id, encTag.encTag);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok(tagExists);
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


        //proxy endpoints

        [HttpPost("checkTag")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> checkTag([FromBody] string base64tag)
        {
            //we will need to add crypto
            bool fileExists = await _fileService.CheckTagAvailabilityInCloud(base64tag);
            return Ok(fileExists);
        }

        [HttpPost("getChallenge")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> getChallengeForTag([FromBody] string base64tag)
        {
            FileMetaChallenge? fmc = await _fileService.GetChallengeForTag(base64tag);
            if(fmc == null)
                return BadRequest("Something went wrong!");
            return Ok(fmc); 
        }

        [HttpPost("verifyFileChallengeForProxy")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> GetChallengeResponseForProxy([FromBody] FileResp fr)
        {
            bool result = await _fileService.VerifyChallengeResponseFromProxy(fr);
            if (result == false)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("getDecryptedFileParams")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> GetDecryptedFileParams([FromBody] FileEncDataDto filEncData)
        {
            try
            {
                FileDecDataDto fileDecData = await _fileService.GetDecryptedFileParams(filEncData);
                return Ok(fileDecData);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("saveDeduplicateFileForUser")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> SaveDedupFileForUser([FromBody] FileDedupDto fileDedupDto)
        {
            try
            {
                await _fileService.SaveDedupFile(fileDedupDto);
                return Ok("File uploaded");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("saveFileFromCache")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> SaveFileFromCache([FromBody] FileFromCacheDto cacheFiles)
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return BadRequest("Authorization Problems!");
            }
            try
            {
                await _fileService.SaveFileFromCache(cacheFiles);
                return Ok("File uploaded");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost("getUploadedFileNamesAndDatesWithoutProxy")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> GetFileNamesAndDates([FromBody] string userEmail)
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return BadRequest("Authorization Problems!");
            }
            string userId = await _userService.GetUserIdByEmail(userEmail);
            List<FilesNameDate>? result = await _fileService.GetFileNamesAndDatesOfUser(userId);

            return Ok(result);
        }
    }
}
