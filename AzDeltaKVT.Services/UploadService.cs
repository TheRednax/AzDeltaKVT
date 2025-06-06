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
