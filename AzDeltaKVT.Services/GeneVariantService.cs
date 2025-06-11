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

        public async Task<IList<GeneVariant>> Find()
        {
            return await _context.GeneVariants.ToListAsync();
        }

        public async Task<GeneVariant?> Get(string nmId, int variantId)
        {
            return await _context.GeneVariants
                .FirstOrDefaultAsync(gv => gv.NmId == nmId && gv.VariantId == variantId);
        }

        public async Task<GeneVariantResult> Create(GeneVariantRequest request)
        {
            // 1. Maak eerst de Variant aan
            var variant = new Variant
            {
                Chromosome = request.Variant.Chromosome,
                Position = request.Variant.Position ?? 0,
                Reference = request.Variant.Reference,
                Alternative = request.Variant.Alternative,
                UserInfo = request.Variant.UserInfo

            };

            _context.Variants.Add(variant);
            await _context.SaveChangesAsync(); // Slaat Variant op en genereert VariantId

            // 2. Koppel deze variant aan een nieuwe GeneVariant
            var geneVariant = new GeneVariant
            {
                NmId = request.NmId,
                VariantId = variant.VariantId, // FK-relatie
                BiologicalEffect = request.BiologicalEffect,
                Classification = request.Classification,
                UserInfo = request.UserInfo
            };

            _context.GeneVariants.Add(geneVariant);
            await _context.SaveChangesAsync();

            // 3. Geef de response terug
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
