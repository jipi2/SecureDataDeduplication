﻿using FileStorageApp.Server.Services;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using System.Net;
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


        [HttpPost("deleteFile")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> deleteFile([FromBody] EmailFilenameDto paramsDto)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }

                string id = await _userService.GetUserIdByEmail(paramsDto.userEmail);
                await _fileService.DeleteFile(id, paramsDto.fileName);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok("File deleted");
        }

        [HttpPost("getPubKeyAndFileKey")]
        [Authorize]
        public async Task<IActionResult> getRsaPubKeyAndFileKey([FromBody] EmailFilenameDto ef)
        {
            try 
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }

                string id = await _userService.GetUserIdFromJWT(token);
                RsaKeyFileKeyDto dto =  await _fileService.GetRsaPubKeyAndFileKey(ef, id);
                return Ok(dto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpPost("sendFile")]
        [Authorize]
        public async Task<IActionResult> sendFile([FromBody] FileTransferDto ftdto)
        {
            try
            {
                string? token = ftdto.senderToken;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                await _fileService.SendFile(ftdto, id);
                return Ok("File send!");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("getRecievedFiles")]
        [Authorize]
        public async Task<IActionResult> getRecievedFiles()
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }

                string id = await _userService.GetUserIdFromJWT(token);
                List<RecievedFilesDto>? rf = await _fileService.GetRecievedFiles(id);
                if(rf == null)
                    rf = new List<RecievedFilesDto>();

                return Ok(rf);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("verifyFileTransfer")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> verifyFileTransfer([FromBody] TransferVerificationDto tvd)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                bool isInCache = await _fileService.VerifyFileTransfer(tvd);
                return Ok(isInCache);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("removeRecievedFile")]
        [Authorize]
        public async Task<IActionResult> removeRecievedFile([FromBody] RecievedFilesDto rfd)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }

                string id = await _userService.GetUserIdFromJWT(token);
                await _fileService.RemoveRecievedFile(rfd, id);
                return Ok("File removed");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("acceptRecievedFile")]
        [Authorize]
        public async Task<IActionResult> acceptRecievedFile([FromBody] AcceptFileTransferDto aft)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }

                await _fileService.AcceptRecievedFile(aft);
                return Ok("File recieved");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("deleteFileTransfer")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> deleteFileTransfer([FromBody] TransferVerificationDto tvt)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }

                await _fileService.DeleteFileTransfer(tvt);
                return Ok("File deleted");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }   
        }

        [HttpPost("proxyGetUrlFileFromStorage")]
        [Authorize(Roles = "proxy")]
        public async Task<BlobFileParamsDto> GetUrlFileFromStorage([FromBody] EmailFilenameDto paramsDto)
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return null;
            }

            string id = await _userService.GetUserIdByEmail(paramsDto.userEmail);
            BlobFileParamsDto severFile = await _fileService.GetUrlFileFromStorage(id, paramsDto.fileName);
            return severFile;
        }

        [HttpPost("getKeyAndIvForFile")]
        [Authorize]
        public async Task<FileKeyAndIvDto> GetKeyAndIvForFile([FromBody] EmailFilenameDto paramsDto)
        {
            string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
            if (token.IsNullOrEmpty())
            {
                return null;
            }

            string id = await _userService.GetUserIdByEmail(paramsDto.userEmail);
            FileKeyAndIvDto dto = await _fileService.GetKeyAndIvForFile(id, paramsDto.fileName);
            return dto;
        }


        [HttpPost("writeFileOnDisk")]
        [Authorize]
        public async Task<IActionResult> SaveFileFromCache_v2(IFormFile file)
        {
            try
            {
                string? tag = Request.Headers["base64Tag"].ToString();
                if (tag == null)
                    throw new Exception("The tag was not sent in header");
                await _fileService.WriteFileOnDiskAndOnCloud(file, tag);
                return Ok("Chunk Uploaded");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("saveFileFromCacheParams")]
        [Authorize]
        public async Task<IActionResult> SaveFileParams([FromBody] FileFromCacheDto_v2 fileParams)
        {
            try
            {
                await _fileService.SaveFileFromCache(fileParams);
                return Ok("Data ok");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if(ex.Message.Equals("Url for Azure Blob Stroage is null"))
                    return Ok("Data ok");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("saveFileInfoFromCache")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> SaveFileInfoFromCache([FromBody] FileInfoFromCache dto)
        {
            try
            {
                await _fileService.SaveFileInfoFromCache(dto);
                return Ok("Data saved");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("deleteFileInfoFromServer")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> DeleteFileInfoFromServer([FromBody] DeleteFileInfoDto dto)
        {
            try
            {
                await _fileService.DeleteFileInfoFromServer(dto);
                return Ok("Data deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("saveFileInfoRecievedFromAnotherUser")]
        [Authorize(Roles = "proxy")]
        public async Task<IActionResult> DeleteFileInfoFromServer([FromBody] AcceptFileTransferDto aft)
        {
            try
            {
                await _fileService.SaveFileInfoRecievedFromAnotherUser(aft);
                return Ok("File Info saved");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("verifyNameDuplicate")]
        [Authorize]
        public async Task<IActionResult> VerifyNameDuplicate([FromBody] string fullPathName)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return null;
                }

                string id = await _userService.GetUserIdFromJWT(token);
                bool result = await _fileService.VerifyNameDuplicate(id, fullPathName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("renameFile")]
        [Authorize]
        public async Task<IActionResult> RenameFile([FromBody] RenameFileDto rfd)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return null;
                }

                string id = await _userService.GetUserIdFromJWT(token);
                await _fileService.RenameFile(id, rfd);
                return Ok("File renamed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("renameFolder")]
        [Authorize]
        public async Task<IActionResult> RenameFolder([FromBody] RenameFileDto rfd)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return null;
                }

                string id = await _userService.GetUserIdFromJWT(token);
                await _fileService.RenameFolder(id, rfd);
                return Ok("Folder renamed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

}
