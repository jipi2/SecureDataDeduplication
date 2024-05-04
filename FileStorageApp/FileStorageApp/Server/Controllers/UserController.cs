using FileStorageApp.Server.Services;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Bcpg.Sig;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace FileStorageApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        public UserController(UserService userService, EmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        [HttpGet("testMail")]
        [AllowAnonymous]
        public async Task<IActionResult> TestMail()
        {
            try
            {
                _emailService.SendEmail("jipianu_mihnea@yahoo.com", "TEST", "asat este doar un body");
                return Ok("email sent");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
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

        [HttpPost("sendEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> sendEmail([FromBody] string email)
        {
            try
            {
                await _userService.SendVerificationEmailToLoggedUser(email);
                return Ok("Email sent!");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("verifyCode")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(VerifyCodeDto dto)
        {
            try
            {
                Response resp = await _userService.VerifyCode(dto);
                return Ok(resp);
            }
            catch (ExceptionModel e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost("resetPassword")]
        [Authorize]
        public async Task<IActionResult> ResetPassword(ChangePasswordDto dto)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                await _userService.ResetPassword(id, dto);
                return Ok("The password has been updated!");
            }
            catch (Exception e)
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


        [HttpPost("getUserRsaPubKey")]
        [Authorize]
        public async Task<IActionResult> getUserPubKey([FromBody] string userEmail)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string? userKey = await _userService.GetUserPubKey(userEmail);
                if(userKey.IsNullOrEmpty())
                {
                    return BadRequest("User not found");
                }
                return Ok(userKey);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("getRsaKeyPair")]
        [Authorize]
        public async Task<IActionResult> getUserRsaKeys()
        {
            try 
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                RsaDto? rsaDto = await _userService.GetRsaKeyPair(id);
                if(rsaDto == null)
                {
                    return BadRequest("User does not have RSA key pair");
                }
                return Ok(rsaDto);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("getPubKeyForFileTransfer")]
        [Authorize]
        public async Task<IActionResult> GetRecieverPubKey([FromBody] string receiverEmail)
        {
            try 
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                string base64PubKey = await _userService.GetRecieverPubKey(receiverEmail);
                return Ok(base64PubKey);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("getKfragFromReciever")]
        [Authorize]
        public async Task<IActionResult> GetKeyFragFromReceiver([FromBody] string receiverEmail)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                string base64KFrag = await _userService.GetRecieverKFrag(id,receiverEmail);
                return Ok(base64KFrag);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
        }

        [HttpPost("saveKFragForReceiver")]
        [Authorize]
        public async Task<IActionResult> SaveKFrag([FromBody] KFragDto dto)
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                await _userService.SaveKFrag(id, dto);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("isConnected")]
        [Authorize]
        public IActionResult IsJWTValid()
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                return Ok("Token is Valid");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("getFirstAndLastName")]
        [Authorize]
        public async Task<IActionResult> GetFirstAndLastName()
        {
            try
            {
                string? token = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]).Parameter;
                if (token.IsNullOrEmpty())
                {
                    return BadRequest("Token invalid");
                }
                string id = await _userService.GetUserIdFromJWT(token);
                return Ok(await _userService.GetFirstAndLastName(id));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
