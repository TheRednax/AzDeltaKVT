using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;

namespace AzDeltaKVT.Services
{
	public class UploadService
	{
		private readonly AzDeltaKVTDbContext _dbContext;

		public UploadService(AzDeltaKVTDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<UploadResult> UploadTsvFile(UploadRequest request)
		{
			try
			{
				var result = new UploadResult
				{
					Errors = new List<string>(),
					Rows = new List<UploadRowResult>()
				};
				var tsvVariants = new List<TsvFileRow>();

				using var stream = request.TsvFile.OpenReadStream();
				using var reader = new StreamReader(stream);

				int lineNumber = 0;
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					lineNumber++;

					// Skip header
					if (lineNumber == 1 && line.StartsWith("chromosome", StringComparison.OrdinalIgnoreCase))
						continue;

					if (string.IsNullOrWhiteSpace(line))
						continue;

					var parts = line.Split('\t');

					if (parts.Length != 4)
					{
						result.Errors!.Add($"Line {lineNumber}: Incorrect number of fields");
						continue;
					}

					try
					{
						var variant = new TsvFileRow()
						{
							Chromosome = parts[0],
							Position = int.Parse(parts[1]),
							Reference = parts[2],
							Alternative = parts[3]
						};

						tsvVariants.Add(variant);
					}
					catch (Exception e)
					{
						result.Errors!.Add($"Line {lineNumber}: {e.Message}");
					}
				}

				var variants = _dbContext.GeneVariants
					.Include(gv => gv.Variant)
					.Include(gv => gv.NmTranscript)
					.ThenInclude(nm => nm.Gene)
					.Where(gv => tsvVariants.Select(tsvV => tsvV.Position).Contains(gv.Variant.Position)
								 && gv.NmTranscript.IsInHouse).ToList();

				foreach (var tsvVariant in tsvVariants)
				{
					var existingVariant = variants.FirstOrDefault(v => v.Variant.Position == tsvVariant.Position);
					if (existingVariant != null)
					{
						result.Rows.Add(new UploadRowResult
						{
							Chromosome = existingVariant.Variant.Chromosome,
							Position = existingVariant.Variant.Position,
							Reference = existingVariant.Variant.Reference,
							Alternative = existingVariant.Variant.Alternative,
							GeneName = existingVariant.NmTranscript.Gene.Name,
							NmNumber = existingVariant.NmTranscript.NmNumber,
							IsInHouse = existingVariant.NmTranscript.IsInHouse,
							BiologicalEffect = existingVariant.BiologicalEffect,
							Classification = existingVariant.Classification,
							IsKnownPosition = true
						});
					}
					else
					{
						result.Rows.Add(new UploadRowResult
						{
							Chromosome = tsvVariant.Chromosome,
							Position = tsvVariant.Position,
							Reference = tsvVariant.Reference,
							Alternative = tsvVariant.Alternative,
							GeneName = null,
							NmNumber = null,
							IsInHouse = null,
							BiologicalEffect = null,
							Classification = null,
							IsKnownPosition = false,
						});
					}
				}

				result.Rows = result.Rows.OrderBy(r => r.Position).ToList();

				return result;
			}
			catch (Exception ex)
			{
				return new UploadResult
				{
					Errors = new List<string> { $"Upload failed: {ex.Message}" }
				};
			}
		}
	}
}
