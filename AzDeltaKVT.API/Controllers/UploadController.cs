using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzDeltaKVT.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
	    private readonly UploadService _UploadService;

	    public UploadController(UploadService uploadService)
	    {
            _UploadService = uploadService;
		}

		[HttpPost]
        public IActionResult UploadFile(UploadRequest request)
        {
            _UploadService.UploadTsvFile(request);
			return Ok();
        }

        [HttpGet("results")]
        public IActionResult GetResults()
        {
            return Ok();
        }
    }
}
