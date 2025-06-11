using AzDeltaKVT.Services;
using AzDeltaKVT.Dto.Requests;
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

        // GET /variants
        [HttpGet]
        public async Task<IActionResult> Find()
        {
            var result = await _variantService.Find();
            return Ok(result);
        }

        // POST /variants/get
        [HttpPost("get")]
        public async Task<IActionResult> Get([FromBody] VariantRequest request)
        {
            var variant = await _variantService.Get(request);
            return Ok(variant);
        }

        // POST /variants/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] VariantRequest request)
        {
            var createdVariant = await _variantService.Create(request);
            return Ok(createdVariant);
        }

        // PUT /variants/update
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] VariantRequest request)
        {
            var updated = await _variantService.Update(request);
            return Ok(updated);
        }

        // DELETE /variants/delete
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            var deleted = await _variantService.Delete(id);
            return Ok(deleted);
        }

        // CREATE meerdere variants op 1 Transcript
        [HttpPost("add-to-transcript/{nmNumber}")]
        public async Task<IActionResult> AddVariantToTranscript(string nmNumber, [FromBody] AddVariantToTranscriptRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _variantService.AddVariantToTranscript(nmNumber, request);
                return Created($"/variants/{result.GeneVariant.VariantId}", result);
            }
            catch (ArgumentException ex)
            {
                return Conflict(ex.Message);
            }
        }

    }
}
