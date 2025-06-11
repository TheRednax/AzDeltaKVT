using System.Text;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzDeltaKVT.API.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class UploadController : ControllerBase
	{
		private readonly UploadService _UploadService;

		public UploadController(UploadService uploadService)
		{
			_UploadService = uploadService;
		}

		[HttpPost]
		public async Task<IActionResult> UploadFile( IFormFile tsvFile)
		{
			try
			{
				var request = new UploadRequest { TsvFile = tsvFile };
				var result = await _UploadService.UploadTsvFile(request);

				var sb = new StringBuilder();

				// Voeg een header toe, optioneel
				sb.AppendLine("Chromosome\tPosition\tReference\tAlternative\tGeneName\tNmNumber\tIsInHouse\tBiologicalEffect\tClassification");

				foreach (var variant in result.Rows)
				{
					sb.AppendLine($"{variant.Chromosome}\t{variant.Position}\t{variant.Reference}\t{variant.Alternative}\t{variant.GeneName}\t{variant.NmNumber}\t{variant.IsInHouse}\t{variant.BiologicalEffect}\t{variant.Classification}");
				}

				var byteArray = Encoding.UTF8.GetBytes(sb.ToString());

				var fileName = $"{Path.GetFileNameWithoutExtension(tsvFile.FileName)}_{DateTime.UtcNow:yyyyMMddHHmmss}.tsv"; // Unique filename

				var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

				if (!Directory.Exists(uploadFolder))
					Directory.CreateDirectory(uploadFolder);

				var filePath = Path.Combine(uploadFolder, fileName);

				await System.IO.File.WriteAllBytesAsync(filePath, byteArray);

				// Instead of IFormFile, provide a download URL in the result
				var downloadUrl = Url.Action("DownloadFile", "Upload", new { fileName = fileName }, Request.Scheme);

				result.DownloadUrl = downloadUrl;
				result.FileName = fileName;

				return Ok(result);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		[HttpGet("download/{fileName}")]
		public IActionResult DownloadFile(string fileName)
		{
			var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
			var filePath = Path.Combine(uploadFolder, fileName);

			if (!System.IO.File.Exists(filePath))
				return NotFound();

			var contentType = "text/tab-separated-values";
			var fileBytes = System.IO.File.ReadAllBytes(filePath);
			return File(fileBytes, contentType, fileName);
		}

	}
}
