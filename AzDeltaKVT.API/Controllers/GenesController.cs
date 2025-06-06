using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
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

        // GET /genes
        [HttpGet]
        public async Task<IActionResult> Find()
        {
            var result = await _geneService.Find();
            return Ok(result);
        }

        // POST /genes/get
        [HttpPost("get")]
        public async Task<IActionResult> Get([FromBody]GeneRequest request)
        {
            var genes = await _geneService.Get(request);
            return Ok(genes);
        }

        // POST /genes/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] GeneRequest request)
        {
            var createdGene = await _geneService.Create(request);
            return Ok(createdGene);
        }

        // PUT /genes/update
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] GeneRequest request)
        {
            var updated = await _geneService.Update(request);
            return Ok(updated);
        }

        // DELETE /genes/delete
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] string name)
        {
            var deleted = await _geneService.Delete(name);
            return Ok(deleted);
        }
    }
}