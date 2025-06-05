using AzDeltaKVT.Services;
using AzDektaKVT.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AzDeltaKVT.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GeneVariantsController : ControllerBase
    {
        private readonly GeneVariantService _geneVariantService;

        public GeneVariantsController(GeneVariantService geneVariantService)
        {
            _geneVariantService = geneVariantService;
        }

        // GET /genevariants
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = await _geneVariantService.GetAllAsync();
            return Ok(results);
        }

        // GET /genevariants/{nmId}/{variantId}
        [HttpGet("{nmId}/{variantId}")]
        public async Task<IActionResult> Get(string nmId, int variantId)
        {
            var geneVariant = await _geneVariantService.GetByIdsAsync(nmId, variantId);
            if (geneVariant == null)
                return NotFound();
            return Ok(geneVariant);
        }

        // POST /genevariants
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GeneVariant geneVariant)
        {
            var created = await _geneVariantService.CreateAsync(geneVariant);
            return CreatedAtAction(nameof(Get), new { nmId = created.NmId, variantId = created.VariantId }, created);
        }

        // PUT /genevariants/{nmId}/{variantId}
        [HttpPut("{nmId}/{variantId}")]
        public async Task<IActionResult> Update(string nmId, int variantId, [FromBody] GeneVariant geneVariant)
        {
            if (nmId != geneVariant.NmId || variantId != geneVariant.VariantId)
                return BadRequest("Ids mismatch");

            var updated = await _geneVariantService.UpdateAsync(nmId, variantId, geneVariant);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE /genevariants/{nmId}/{variantId}
        [HttpDelete("{nmId}/{variantId}")]
        public async Task<IActionResult> Delete(string nmId, int variantId)
        {
            var deleted = await _geneVariantService.DeleteAsync(nmId, variantId);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}