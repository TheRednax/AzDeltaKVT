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
		private readonly TranscriptService _transcriptService;
		private readonly VariantService _variantService;
		private readonly GeneVariantService _geneVariantService;

		public GenesController(GeneService geneService, TranscriptService transcriptService, VariantService variantService, GeneVariantService geneVariantService)
		{
			_geneService = geneService;
			_transcriptService = transcriptService;
			_variantService = variantService;
			_geneVariantService = geneVariantService;
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
		public async Task<IActionResult> Get([FromBody] GeneRequest request)
		{
			var genes = await _geneService.Get(request);
			return Ok(genes);
		}

		// POST /genes/create
		[HttpPost("create")]
		public async Task<IActionResult> Create([FromBody] GeneRequest request)
		{
			var transcript = await _transcriptService.Get(request.Nm_Number);
			if (transcript != null)
			{
				return BadRequest(new { message = "Transcript already exists for this NM number." });
			}

			var existingGene = await _geneService.GetByName(request.Name);
			if (existingGene != null)
			{

				if (transcript != null)
				{
					return BadRequest(new { message = "This combination of Transcript number and Gene name already exists." });
				}

				var requestTranscript = new NmTranscript
				{
					GeneId = request.Name,
					IsInHouse = request.IsInHouse,
					IsClinical = request.IsClinical,
					IsSelect = request.IsSelect,
					NmNumber = request.Nm_Number
				};
				var createdTranscript = await _transcriptService.Create(requestTranscript);
				var geneResult = await _geneService.Get(new GeneRequest
				{
					Name = request.Name,
					Nm_Number = request.Nm_Number,
				});
				return Ok(geneResult);
			}

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
			var transcripts = await _transcriptService.Find();
			var transcript = transcripts.FirstOrDefault(t => t.GeneId == name);
			var nmNumber = transcript.NmNumber;
			var geneVariants = await _geneVariantService.Find();
			var geneVariantTranscripts = geneVariants.Where(gv => gv.NmId == nmNumber).ToList();
			if (!(transcript == null || !geneVariantTranscripts.Any()))
			{
				foreach (var geneVariant in geneVariantTranscripts)
				{
					await _variantService.Delete(geneVariant.VariantId);
					await _geneVariantService.Delete(new GeneVariantRequest
					{ VariantId = geneVariant.VariantId, NmId = nmNumber });
				}
			}
			var deleted = await _geneService.Delete(name);
			return Ok(deleted);
		}
	}
}