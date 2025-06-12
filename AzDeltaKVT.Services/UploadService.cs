using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                // Initialize result and list to store parsed TSV rows
                var result = new UploadResult
                {
                    Errors = new List<string>(),
                    Rows = new List<UploadRowResult>()
                };
                var tsvVariants = new List<TsvFileRow>();

                // Open and read the uploaded file
                using var stream = request.TsvFile.OpenReadStream();
                using var reader = new StreamReader(stream);

                int lineNumber = 0;

                // Read file line by line
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lineNumber++;

                    // Skip header if it starts with "chromosome"
                    if (lineNumber == 1 && line.StartsWith("chromosome", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split('\t');

                    // Line must have exactly 4 columns
                    if (parts.Length != 4)
                    {
                        result.Errors!.Add($"Line {lineNumber}: Incorrect number of fields");
                        continue;
                    }

                    try
                    {
                        // Parse TSV values and add to list
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
                        // Report parsing error
                        result.Errors!.Add($"Line {lineNumber}: {e.Message}");
                    }
                }

                // Query existing GeneVariants that match positions in the TSV and are marked as in-house
                var variants = _dbContext.GeneVariants
                    .Include(gv => gv.Variant)
                    .Include(gv => gv.NmTranscript)
                    .ThenInclude(nm => nm.Gene)
                    .Where(gv => tsvVariants.Select(tsvV => tsvV.Position).Contains(gv.Variant.Position ?? 0)
                                 && gv.NmTranscript.IsInHouse)
                    .ToList();

                // Match each TSV row against existing variants
                foreach (var tsvVariant in tsvVariants)
                {
                    var existingVariant = variants.FirstOrDefault(v => v.Variant.Position == tsvVariant.Position);

                    if (existingVariant != null)
                    {
                        // Match found: build detailed row
                        result.Rows.Add(new UploadRowResult
                        {
                            Chromosome = existingVariant.Variant.Chromosome,
                            Position = existingVariant.Variant.Position ?? 0,
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
                        // No match: return raw TSV data
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

                // Sort rows by position before returning
                result.Rows = result.Rows.OrderBy(r => r.Position).ToList();

                return result;
            }
            catch (Exception ex)
            {
                // Return error if whole upload fails
                return new UploadResult
                {
                    Errors = new List<string> { $"Upload failed: {ex.Message}" }
                };
            }
        }
    }
}
