using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proxy.UserInt;
using System.Net.Http.Json;

namespace Proxy.Controllers
{
    [ApiController]
    [Route("api/User")]
    public class UserController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public UserController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLogin loginUser)
        {
            try
            {
                string backendUrl = "https://localhost:7109/api/User/login";
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(backendUrl, loginUser);

                if(response.IsSuccessStatusCode)
                {
                    Response resp = await response.Content.ReadFromJsonAsync<Response>();
                    return Ok(resp);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return BadRequest(error);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
