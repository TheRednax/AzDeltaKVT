using AzDeltaKVT.Services;
using AzDektaKVT.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AzDeltaKVT.Dto.Requests;

namespace AzDeltaKVT.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TranscriptsController : ControllerBase
    {
        private readonly TranscriptService _transcriptService;
        private readonly GeneVariantService _geneVariantService;
        private readonly VariantService _variantService;

		public TranscriptsController(TranscriptService transcriptService, GeneVariantService geneVariantService, VariantService variantService)
        {
            _transcriptService = transcriptService;
            _geneVariantService = geneVariantService;
            _variantService = variantService;
		}

        // GET /transcripts
        [HttpGet]
        public async Task<IActionResult> Find()
        {
            var results = await _transcriptService.Find();
            return Ok(results);
        }

        // POST /transcripts/get
        [HttpPost("get")]
        public async Task<IActionResult> Get([FromBody] NmTranscript request)
        {
            var transcript = await _transcriptService.Get(request.NmNumber);
            if (transcript == null)
                return NotFound();

            return Ok(transcript);
        }

        // POST /transcripts/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] NmTranscript transcript)
        {
            var created = await _transcriptService.Create(transcript);
            return Ok(created);
        }

        // PUT /transcripts/update
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] NmTranscript transcript)
        {
            var updated = await _transcriptService.Update(transcript);
            return Ok(updated);
        }

        // DELETE /transcripts/delete
        [HttpDelete("{nmNumber}")]
        public async Task<IActionResult> Delete(string nmNumber)
        {
            var transcript = await _transcriptService.Get(nmNumber);
            var geneVariants = await _geneVariantService.Find();
            var geneVariantTranscripts = geneVariants.Where(gv => gv.NmId == nmNumber).ToList();
            if (transcript == null || !geneVariantTranscripts.Any())
            {
                return BadRequest("Transcript cannot be deleted because it is associated with a gene variant.");
			}

            foreach (var geneVariant in geneVariantTranscripts)
            {
	            await _variantService.Delete(geneVariant.VariantId);
	            await _geneVariantService.Delete(new GeneVariantRequest
		            { VariantId = geneVariant.VariantId, NmId = nmNumber });
            }

			var deleted = await _transcriptService.Delete(transcript.NmNumber);
            return Ok(deleted);
        }
    }
}