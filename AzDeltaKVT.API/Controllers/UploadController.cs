using System.Text;
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
        public async Task<IActionResult> UploadFile(UploadRequest request)
        {
            var result = await  _UploadService.UploadTsvFile(request);

            var sb = new StringBuilder();

            // Voeg een header toe, optioneel
            sb.AppendLine("Chromosome\tPosition\tReference\tAlternative\tGeneName\tNmNumber\tIsInHouse\tBiologicalEffect\tClassification");

            foreach (var variant in result.Rows)
            {
	            sb.AppendLine($"{variant.Chromosome}\t{variant.Position}\t{variant.Reference}\t{variant.Alternative}\t{variant.GeneName}\t{variant.NmNumber}\t{variant.IsInHouse}\t{variant.BiologicalEffect}\t{variant.Classification}");
            }

            var byteArray = Encoding.UTF8.GetBytes(sb.ToString());
            var streamToSend = new MemoryStream(byteArray);
            var fileName = "TsvTestFile.tsv";


            IFormFile formFile = new FormFile(streamToSend, 0, byteArray.Length, "TsvFile", Path.GetFileName(fileName))
            {
	            Headers = new HeaderDictionary(),
	            ContentType = "text/tab-separated-values"
            };

            result.TsvFile = formFile;
			return Ok(result);
        }

        [HttpGet("results")]
        public IActionResult GetResults()
        {
            return Ok();
        }
    }
}
