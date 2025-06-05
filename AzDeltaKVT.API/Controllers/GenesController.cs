using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using AzDeltaKVT.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AzDeltaKVT.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GenesController : ControllerBase
    {
        private readonly GeneService _geneService;

        public GenesController(GeneService geneService)
        {
            _geneService = geneService;
        }

        // GET /genes?name=abc
        [HttpGet]
        public async Task<IActionResult> GetAllGenes([FromQuery] string? name)
        {
            var genes = await _geneService.GetAllAsync(name);
            return Ok(genes);
        }

        // GET /genes/{name}
        [HttpGet("{name}")]
        public async Task<IActionResult> GetGene(string name)
        {
            var gene = await _geneService.GetByIdAsync(name);
            if (gene == null)
                return NotFound();
            return Ok(gene);
        }

        // POST /genes
        [HttpPost]
        public async Task<IActionResult> CreateGene([FromBody] Gene gene)
        {
            if (gene == null || string.IsNullOrWhiteSpace(gene.Name))
                return BadRequest("Gene name is required.");

            var createdGene = await _geneService.CreateAsync(gene);
            return CreatedAtAction(nameof(GetGene), new { name = createdGene.Name }, createdGene);
        }

        // PUT /genes/{name}
        [HttpPut("{name}")]
        public async Task<IActionResult> UpdateGene(string name, [FromBody] Gene gene)
        {
            if (gene == null || name != gene.Name)
                return BadRequest("Gene name mismatch.");

            var updated = await _geneService.UpdateAsync(name, gene);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE /genes/{name}
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteGene(string name)
        {
            var deleted = await _geneService.DeleteAsync(name);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("testdb")]
        public async Task<IActionResult> TestDbConnection([FromServices] AzDeltaKVTDbContext context)
        {   
            try
            {
                var canConnect = await context.Database.CanConnectAsync();
                return Ok(new { success = canConnect });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}