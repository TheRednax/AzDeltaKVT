using AzDektaKVT.Model;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
using AzDeltaKVT.Services;
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
        public async Task<IActionResult> Find()
        {
            var results = await _geneVariantService.Find();
            return Ok(results);
        }

        // POST /genevariants/get
        [HttpPost("get")]
        public async Task<IActionResult> Get([FromBody] GeneVariantRequest request)
        {
            var geneVariant = await _geneVariantService.Get(request.NmId, request.VariantId);
            if (geneVariant == null)
                return NotFound();

            return Ok(geneVariant);
        }

        // POST /genevariants/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] GeneVariantRequest request)
        {
            var created = await _geneVariantService.Create(request);
            return Ok(created);
        }

        // PUT /genevariants/update
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] GeneVariantRequest request)
        {
            var updated = await _geneVariantService.Update(request);
            return Ok(updated);
        }

        // DELETE /genevariants/delete
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] GeneVariantRequest request)
        {
            var deleted = await _geneVariantService.Delete(request);
            return Ok(deleted);
        }
    }
}