﻿using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Services;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;

namespace FileStorageApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileFolderController : ControllerBase
    {
        private readonly FileFolderService _fileFolderService;
        private readonly UserService _userService;

        public FileFolderController(FileFolderService fileFolderService, UserService userService)
        {
            _fileFolderService = fileFolderService;
            _userService = userService;
        }

        [HttpPost("createFolder")]
        [Authorize]
        public async Task<IActionResult> CreateFolder([FromBody] string fullFolderPath)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                await _fileFolderService.CreateFolder(id,fullFolderPath);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("getFolderWithFiles")]
        [Authorize]
        public async Task<IActionResult> GetFolderWithFiles([FromQuery]string path)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                FolderDto? dto = await _fileFolderService.GetFolderWithFiles(id, path);
                if (dto == null)
                    return BadRequest("The folder was not found");
                else 
                    return Ok(dto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
