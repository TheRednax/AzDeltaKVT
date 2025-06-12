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

	        if(request.Variant.Position < request.NmTranscript.Gene.Start)
	        {
		        return BadRequest("Position is not bigger or equal to start");
	        }

            if(request.Variant.Position > request.NmTranscript.Gene.Stop)
            {
                return BadRequest("Position is not smaller or equal to stop");
			}
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

        // DELETE /genevariants/delete/{variantId}
        [HttpDelete("delete/{variantId}")]
        public async Task<IActionResult> Delete(int variantId)
        {
            var geneVariant = await _geneVariantService.GetByVariantId(variantId); 

            if (geneVariant == null)
            {
                return NotFound($"Geen gene variant gevonden met VariantId {variantId}");
            }

            var request = new GeneVariantRequest
            {
                NmId = geneVariant.NmId,
                VariantId = geneVariant.VariantId
            };

            var deleted = await _geneVariantService.Delete(request);
            return Ok(deleted);
        }
    }
}