using System.Collections.Generic;
using System.Linq;
using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;

namespace AzDeltaKVT.Services
{
    public class GeneVariantService
    {
        private readonly AzDeltaKVTDbContext _context;

        public GeneVariantService(AzDeltaKVTDbContext context)
        {
            _context = context;
        }

        public IEnumerable<GeneVariant> GetAll()
        {
            return _context.GeneVariants
                .ToList();
        }

        public GeneVariant? GetByIds(string nmId, int variantId)
        {
            return _context.GeneVariants
                .FirstOrDefault(gv => gv.NmId == nmId && gv.VariantId == variantId);
        }

        public GeneVariantResult Create(GeneVariantRequest request)
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
            _context.SaveChanges();
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

        public bool Update(GeneVariantRequest request)
        {
            var existing = GetByIds(request.NmId, request.VariantId);
            if (existing == null) return false;

            existing.BiologicalEffect = request.BiologicalEffect;
            existing.Classification = request.Classification;
            existing.UserInfo = request.UserInfo;

            _context.SaveChanges();
            return true;
        }

        public bool Delete(GeneVariantRequest request)
        {
            var existing = GetByIds(request.NmId, request.VariantId);
            if (existing == null) return false;

            _context.GeneVariants.Remove(existing);
            _context.SaveChanges();
            return true;
        }
    }
}
