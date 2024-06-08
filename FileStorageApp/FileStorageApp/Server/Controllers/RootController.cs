using FileStorageApp.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageApp.Server.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Merge");
        }
    }

    [ApiController]
    [Route("/api")]
    public class ApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Mergem si api");
        }
    }
}
