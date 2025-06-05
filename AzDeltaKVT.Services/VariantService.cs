using AzDeltaKVT.Core;
using AzDektaKVT.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzDeltaKVT.Services
{
    public class VariantService
    {
        private readonly AzDeltaKVTDbContext _context;

        public VariantService(AzDeltaKVTDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Variant>> SearchAsync(string? chrom, int? position, int? geneId)
        {
            var query = _context.Variants.AsQueryable();

            if (!string.IsNullOrEmpty(chrom))
                query = query.Where(v => v.Chromosome == chrom);

            if (position.HasValue)
                query = query.Where(v => v.Position == position.Value);

            // Assuming you have GeneId property in Variant (currently missing)
            if (geneId.HasValue)
            {
                // If Variant had a GeneId property:
                // query = query.Where(v => v.GeneId == geneId.Value);

                // Otherwise you might join to Gene or filter differently.
            }

            return await query.ToListAsync();
        }

        public async Task<Variant?> GetByIdAsync(int id)
        {
            return await _context.Variants.FindAsync(id);
        }

        public async Task<Variant> CreateAsync(Variant variant)
        {
            _context.Variants.Add(variant);
            await _context.SaveChangesAsync();
            return variant;
        }

        public async Task<bool> UpdateAsync(int id, Variant variant)
        {
            var existing = await _context.Variants.FindAsync(id);
            if (existing == null) return false;

            existing.Chromosome = variant.Chromosome;
            existing.Position = variant.Position;
            existing.Alternative = variant.Alternative;
            existing.Reference = variant.Reference;
            existing.UserInfo = variant.UserInfo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var variant = await _context.Variants.FindAsync(id);
            if (variant == null) return false;

            _context.Variants.Remove(variant);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
