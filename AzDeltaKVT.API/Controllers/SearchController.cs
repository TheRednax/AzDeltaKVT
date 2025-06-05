using Microsoft.AspNetCore.Mvc;

namespace AzDeltaKVT.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public IActionResult Search([FromQuery] string? gene, [FromQuery] string? nm, [FromQuery] string? chrom, [FromQuery] int? position)
        {
            if (!string.IsNullOrEmpty(gene))
            {
                // search by gene
                return Ok($"Searching by gene: {gene}");
            }

            if (!string.IsNullOrEmpty(nm))
            {
                // search by NM
                return Ok($"Searching by NM number: {nm}");
            }

            if (!string.IsNullOrEmpty(chrom) && position.HasValue)
            {
                // search by chromosome + position
                return Ok($"Searching by chromosome {chrom} and position {position}");
            }

            return BadRequest("Please provide a valid search query (gene, nm, or chrom + position).");
        }
    }
}
