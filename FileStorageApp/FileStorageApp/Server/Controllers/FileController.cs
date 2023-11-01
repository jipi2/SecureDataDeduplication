using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        [HttpPost("upload")]
        [Authorize(Roles ="client ")]
        public async Task<IActionResult> Upload(IFormFile file)
        {

            return Ok("Merge frate, are atata lungime: "+file.Length.ToString());
        }
    }
}
