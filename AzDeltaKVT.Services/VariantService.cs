using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
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

        public async Task<IList<VariantResult>> Find()
        {
            var query = _context.Variants
                .Select(v => new VariantResult
                {
                    VariantId = v.VariantId,
                    Chromosome = v.Chromosome,
                    Position = v.Position,
                    Reference = v.Reference,
                    Alternative = v.Alternative,
                    UserInfo = v.UserInfo
                });

            return await query.ToListAsync();
        }

        public async Task<VariantResult> Get(VariantRequest request)
        {
            var query = _context.Variants.AsQueryable();

            if (request.VariantId != 0)
            {
                query = query.Where(v => v.VariantId == request.VariantId);
            }
            else if (!string.IsNullOrEmpty(request.Chromosome) && request.Position != 0)
            {
                query = query.Where(v =>
                    v.Chromosome == request.Chromosome &&
                    v.Position == request.Position);
            }
            else
            {
                return new VariantResult(); // No valid filter
            }

            var result = await query
                .Select(v => new VariantResult
                {
                    VariantId = v.VariantId,
                    Chromosome = v.Chromosome,
                    Position = v.Position,
                    Reference = v.Reference,
                    Alternative = v.Alternative,
                    UserInfo = v.UserInfo
                })
                .FirstOrDefaultAsync();

            return result ?? new VariantResult();
        }

        public async Task<VariantResult> Create(VariantRequest request)
        {
            var variant = new Variant
            {
                Chromosome = request.Chromosome,
                Position = request.Position,
                Reference = request.Reference,
                Alternative = request.Alternative,
                UserInfo = request.UserInfo
            };

            _context.Variants.Add(variant);
            await _context.SaveChangesAsync();

            return new VariantResult
            {
                VariantId = variant.VariantId,
                Chromosome = variant.Chromosome,
                Position = variant.Position,
                Reference = variant.Reference,
                Alternative = variant.Alternative,
                UserInfo = variant.UserInfo
            };
        }

        public async Task<VariantResult> Update(VariantRequest request)
        {
            var existing = await _context.Variants.FindAsync(request.VariantId);
            if (existing == null)
                return null;

            existing.Chromosome = request.Chromosome;
            existing.Position = request.Position;
            existing.Reference = request.Reference;
            existing.Alternative = request.Alternative;
            existing.UserInfo = request.UserInfo;

            await _context.SaveChangesAsync();

            return new VariantResult
            {
                VariantId = existing.VariantId,
                Chromosome = existing.Chromosome,
                Position = existing.Position,
                Reference = existing.Reference,
                Alternative = existing.Alternative,
                UserInfo = existing.UserInfo
            };
        }

        public async Task<bool> Delete(int id)
        {
            var variant = await _context.Variants.FindAsync(id);
            if (variant == null)
                return false;

            _context.Variants.Remove(variant);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
