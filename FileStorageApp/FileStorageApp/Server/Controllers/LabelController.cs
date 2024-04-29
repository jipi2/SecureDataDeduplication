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
    public class LabelController : ControllerBase
    {
        LabelService _labelService;
        UserService _userService;
        public LabelController(LabelService labelService, UserService userService)
        {
            this._labelService = labelService;
            this._userService = userService;
        }

        [HttpPost("createLabel")]
        [Authorize]
        public async Task<IActionResult> CreateLabel([FromBody] string labelName)
        {
            try
            {
                string token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Invalid toke");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                await _labelService.CreateLabel(id, labelName);
                return Ok();
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("deleteLabel")]
        [Authorize]
        public async Task<IActionResult> DeleteLabel([FromBody] string labelName)
        {
            try
            {
                string token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Invalid toke");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                await _labelService.RemoveLabel(id, labelName);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("addLabelToFile")]
        [Authorize]
        public async Task<IActionResult> AddLabelToFile([FromBody] AddLabelToFileDto dto)
        {
            try
            {
                string token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Invalid toke");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                await _labelService.AddLabelToFile(id, dto);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("removeLabelFile")]
        [Authorize]
        public async Task<IActionResult> RemoveLabelFile([FromBody] RemoveLabelFileDto dto)
        {
            try
            {
                string token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Invalid toke");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                await _labelService.RemoveLabelFile(id, dto);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("GetLabelsForUser")]
        [Authorize]
        public async Task<IActionResult> GetLabelsForUser()
        {
            try
            {
                string token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Invalid toke");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                LabelsDto lablesLists = await _labelService.GetLabelsForUser(id);
                return Ok(lablesLists);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}
