using AzDektaKVT.Model;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
using AzDeltaKVT.Services;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Find()
        {
            var results = _geneVariantService.GetAll();
            return Ok(results);
        }

        // POST /genevariants/get
        [HttpPost("get")]
        public IActionResult Get([FromBody] GeneVariantRequest request)
        {
            var geneVariant = _geneVariantService.GetByIds(request.NmId, request.VariantId);
            if (geneVariant == null)
                return NotFound();

            return Ok(geneVariant);
        }

        // POST /genevariants/create
        [HttpPost("create")]
        public IActionResult Create([FromBody] GeneVariantRequest request)
        {
            var created = _geneVariantService.Create(request);
            return Ok(created);
        }

        // PUT /genevariants/update
        [HttpPut("update")]
        public IActionResult Update([FromBody] GeneVariantRequest request)
        {
            var updated = _geneVariantService.Update(request);
            return Ok(updated);
        }

        // DELETE /genevariants/delete
        [HttpDelete("delete")]
        public IActionResult Delete([FromBody] GeneVariantRequest request)
        {
            var deleted = _geneVariantService.Delete(request);
            return Ok(deleted);
        }
    }
}
