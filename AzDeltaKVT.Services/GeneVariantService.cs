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

        // Get all gene variants including related variant and transcript data
        public async Task<IList<GeneVariant>> Find()
        {
            return await _context.GeneVariants
                .Include(gv => gv.Variant)
                .Include(gv => gv.NmTranscript)
                .ToListAsync();
        }

        // Get a specific gene variant by NM ID and variant ID
        public async Task<GeneVariant?> Get(string nmId, int variantId)
        {
            return await _context.GeneVariants
                .Include(gv => gv.Variant)
                .Include(gv => gv.NmTranscript)
                .FirstOrDefaultAsync(gv => gv.VariantId == variantId);
        }

        // Create a new variant and its associated gene variant
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

        // Update an existing gene variant if it exists
        public async Task<bool> Update(GeneVariantRequest request)
        {
            var existing = await Get(request.NmId, request.VariantId);
            if (existing == null) return false;

            existing.BiologicalEffect = request.BiologicalEffect;
            existing.Classification = request.Classification;
            existing.UserInfo = request.UserInfo;

            await _context.SaveChangesAsync();
            return true;
        }

        // Delete an existing gene variant if it exists
        public async Task<bool> Delete(GeneVariantRequest request)
        {
            var existing = await Get(request.NmId, request.VariantId);
            if (existing == null) return false;

            _context.GeneVariants.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
