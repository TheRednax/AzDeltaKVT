using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
using Microsoft.EntityFrameworkCore;

namespace AzDeltaKVT.Services
{
    public class GeneVariantService
    {
        private readonly AzDeltaKVTDbContext _context;

        public GeneVariantService(AzDeltaKVTDbContext context)
        {
            _context = context;
        }

        // Get all gene variants, including related variant and transcript data
        public async Task<IList<GeneVariant>> Find()
        {
            return await _context.GeneVariants
                .Include(gv => gv.Variant)
                .Include(gv => gv.NmTranscript)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get a specific gene variant by variant ID or NM ID
        public async Task<GeneVariant?> Get(string nmId, int variantId)
        {
            if (variantId > 0)
            {
                return await _context.GeneVariants
                    .Include(gv => gv.Variant)
                    .Include(gv => gv.NmTranscript)
                    .FirstOrDefaultAsync(gv => gv.VariantId == variantId);
            }
            else if (!string.IsNullOrEmpty(nmId))
            {
                // Get the first gene variant for the given NM ID (can be adjusted with more filters)
                return await _context.GeneVariants
                    .Include(gv => gv.Variant)
                    .Include(gv => gv.NmTranscript)
                    .Where(gv => gv.NmId == nmId)
                    .OrderBy(gv => gv.VariantId)
                    .FirstOrDefaultAsync();
            }

            return null;
        }

        // Create a new gene variant, including the underlying variant
        public async Task<GeneVariantResult> Create(GeneVariantRequest request)
        {
            var variant = new Variant
            {
                Chromosome = request.Variant.Chromosome,
                Position = request.Variant.Position ?? 0,
                Reference = request.Variant.Reference,
                Alternative = request.Variant.Alternative,
                UserInfo = request.Variant.UserInfo
            };

            _context.Variants.Add(variant);
            await _context.SaveChangesAsync();

            var geneVariant = new GeneVariant
            {
                NmId = request.NmId,
                VariantId = variant.VariantId,
                BiologicalEffect = request.BiologicalEffect,
                Classification = request.Classification,
                UserInfo = request.UserInfo
            };

            _context.GeneVariants.Add(geneVariant);
            await _context.SaveChangesAsync();

            return new GeneVariantResult
            {
                VariantId = geneVariant.VariantId,
                NmId = geneVariant.NmId,
                NmTranscript = geneVariant.NmTranscript,
                BiologicalEffect = geneVariant.BiologicalEffect,
                Classification = geneVariant.Classification,
                UserInfo = geneVariant.UserInfo
            };
        }

        // Update an existing gene variant and its associated variant data
        public async Task<bool> Update(GeneVariantRequest request)
        {
            var existing = await Get(request.NmId, request.VariantId);
            if (existing == null) return false;

            // Re-attach to enable EF Core tracking
            _context.Attach(existing);
            _context.Entry(existing).State = EntityState.Modified;

            // Update variant fields if available
            if (existing.Variant != null)
            {
                _context.Attach(existing.Variant);
                _context.Entry(existing.Variant).State = EntityState.Modified;

                existing.Variant.Chromosome = request.Variant.Chromosome;
                existing.Variant.Position = request.Variant.Position;
                existing.Variant.Reference = request.Variant.Reference;
                existing.Variant.Alternative = request.Variant.Alternative;
                existing.Variant.UserInfo = request.Variant.UserInfo;
            }

            // Update gene variant fields
            existing.BiologicalEffect = request.BiologicalEffect;
            existing.Classification = request.Classification;
            existing.UserInfo = request.UserInfo;

            await _context.SaveChangesAsync();
            return true;
        }

        // Delete a gene variant and its associated variant if they exist
        public async Task<bool> Delete(GeneVariantRequest request)
        {
            var existing = await Get(request.NmId, request.VariantId);
            if (existing == null) return false;

            // Remove the associated variant entity
            if (existing.Variant != null)
            {
                _context.Variants.Remove(existing.Variant);
            }

            // Remove the gene variant itself
            _context.GeneVariants.Remove(existing);

            await _context.SaveChangesAsync();
            return true;
        }

        // Get a gene variant by its variant ID only
        public async Task<GeneVariant?> GetByVariantId(int variantId)
        {
            return await _context.GeneVariants
                .Include(gv => gv.Variant)
                .FirstOrDefaultAsync(gv => gv.VariantId == variantId);
        }
    }
}
