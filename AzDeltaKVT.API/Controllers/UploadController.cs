using Microsoft.AspNetCore.Mvc;

namespace AzDeltaKVT.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            return Ok();
        }

        [HttpGet("results")]
        public IActionResult GetResults()
        {
            return Ok();
        }
    }
}
