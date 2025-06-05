using AzDeltaKVT.Services;
using AzDektaKVT.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AzDeltaKVT.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TranscriptsController : ControllerBase
    {
        private readonly TranscriptService _transcriptService;

        public TranscriptsController(TranscriptService transcriptService)
        {
            _transcriptService = transcriptService;
        }

        // GET /transcripts/{nmNumber}
        [HttpGet("{nmNumber}")]
        public async Task<IActionResult> GetTranscript(string nmNumber)
        {
            var transcript = await _transcriptService.GetByIdAsync(nmNumber);
            if (transcript == null)
                return NotFound();
            return Ok(transcript);
        }

        // PUT /transcripts/{nmNumber}
        [HttpPut("{nmNumber}")]
        public async Task<IActionResult> UpdateTranscript(string nmNumber, [FromBody] NmTranscript transcript)
        {
            if (nmNumber != transcript.NmNumber)
                return BadRequest("NmNumber mismatch");

            var updated = await _transcriptService.UpdateAsync(nmNumber, transcript);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE /transcripts/{nmNumber}
        [HttpDelete("{nmNumber}")]
        public async Task<IActionResult> DeleteTranscript(string nmNumber)
        {
            var deleted = await _transcriptService.DeleteAsync(nmNumber);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
