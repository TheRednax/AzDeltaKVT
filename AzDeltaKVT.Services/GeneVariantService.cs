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
            return await _context.GeneVariants
                .Include(gv => gv.Variant)
                .Include(gv => gv.NmTranscript)
                .AsNoTracking()
                .ToListAsync();
        }

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
                // You could try to find the first variant for that NmId (or add more filters if needed)
                return await _context.GeneVariants
                    .Include(gv => gv.Variant)
                    .Include(gv => gv.NmTranscript)
                    .Where(gv => gv.NmId == nmId)
                    .OrderBy(gv => gv.VariantId) // or order by something meaningful
                    .FirstOrDefaultAsync();
            }

            return null;
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

            // Reattach it so EF can track again
            _context.Attach(existing);
            _context.Entry(existing).State = EntityState.Modified;

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

            // Remove linked Variant entity
            if (existing.Variant != null)
            {
                _context.Variants.Remove(existing.Variant);
            }

            // Remove the GeneVariant entity
            _context.GeneVariants.Remove(existing);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GeneVariant?> GetByVariantId(int variantId)
        {
            return await _context.GeneVariants
                .Include(gv => gv.Variant)
                .FirstOrDefaultAsync(gv => gv.VariantId == variantId);
        }
    }
}
