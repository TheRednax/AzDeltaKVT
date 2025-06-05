using AzDeltaKVT.Services;
using AzDektaKVT.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AzDeltaKVT.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VariantsController : ControllerBase
    {
        private readonly VariantService _variantService;

        public VariantsController(VariantService variantService)
        {
            _variantService = variantService;
        }

        // GET /variants?chrom=17&position=41276045&geneId=123
        [HttpGet]
        public async Task<IActionResult> SearchVariants([FromQuery] string? chrom, [FromQuery] int? position, [FromQuery] int? geneId)
        {
            var variants = await _variantService.SearchAsync(chrom, position, geneId);
            return Ok(variants);
        }

        // GET /variants/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVariant(int id)
        {
            var variant = await _variantService.GetByIdAsync(id);
            if (variant == null)
                return NotFound();
            return Ok(variant);
        }

        // POST /variants
        [HttpPost]
        public async Task<IActionResult> CreateVariant([FromBody] Variant variant)
        {
            var created = await _variantService.CreateAsync(variant);
            return CreatedAtAction(nameof(GetVariant), new { id = created.VariantId }, created);
        }

        // PUT /variants/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVariant(int id, [FromBody] Variant variant)
        {
            if (id != variant.VariantId)
                return BadRequest("Id mismatch");

            var updated = await _variantService.UpdateAsync(id, variant);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE /variants/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVariant(int id)
        {
            var deleted = await _variantService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}