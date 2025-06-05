using AzDeltaKVT.Core;
using AzDektaKVT.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace AzDeltaKVT.Services
{
    public class GeneVariantService
    {
        private readonly AzDeltaKVTDbContext _context;

        public GeneVariantService(AzDeltaKVTDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GeneVariant>> GetAllAsync()
        {
            return await _context.GeneVariants
                .Include(gv => gv.NmTranscript)
                .Include(gv => gv.Variant)
                .ToListAsync();
        }

        public async Task<GeneVariant?> GetByIdsAsync(string nmId, int variantId)
        {
            return await _context.GeneVariants
                .Include(gv => gv.NmTranscript)
                .Include(gv => gv.Variant)
                .FirstOrDefaultAsync(gv => gv.NmId == nmId && gv.VariantId == variantId);
        }

        public async Task<GeneVariant> CreateAsync(GeneVariant geneVariant)
        {
            _context.GeneVariants.Add(geneVariant);
            await _context.SaveChangesAsync();
            return geneVariant;
        }

        public async Task<bool> UpdateAsync(string nmId, int variantId, GeneVariant geneVariant)
        {
            var existing = await GetByIdsAsync(nmId, variantId);
            if (existing == null) return false;

            existing.BiologicalEffect = geneVariant.BiologicalEffect;
            existing.Classification = geneVariant.Classification;
            existing.UserInfo = geneVariant.UserInfo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string nmId, int variantId)
        {
            var existing = await GetByIdsAsync(nmId, variantId);
            if (existing == null) return false;

            _context.GeneVariants.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}