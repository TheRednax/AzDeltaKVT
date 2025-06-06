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
            var result = new GeneVariant
            {
                NmId = request.NmId,
                VariantId = request.VariantId,
                BiologicalEffect = request.BiologicalEffect,
                Classification = request.Classification,
                UserInfo = request.UserInfo
            };

            _context.GeneVariants.Add(result);
            await _context.SaveChangesAsync();

            return new GeneVariantResult
            {
                VariantId = result.VariantId,
                NmId = result.NmId,
                NmTranscript = result.NmTranscript,
                BiologicalEffect = result.BiologicalEffect,
                Classification = result.Classification,
                UserInfo = result.UserInfo
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
