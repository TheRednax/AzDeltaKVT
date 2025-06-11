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

        public async Task<AddVariantToTranscriptResult> AddVariantToTranscript(string nmNumber, AddVariantToTranscriptRequest request)
        {
            // 1. Check transcript exists
            var transcript = await _context.NmTranscripts
                .Include(t => t.Gene)
                .FirstOrDefaultAsync(t => t.NmNumber == nmNumber);
            if (transcript == null)
                throw new ArgumentException($"Transcript {nmNumber} not found");

            // 2. Check/Create variant
            var existingVariant = await _context.Variants
                .FirstOrDefaultAsync(v =>
                    v.Chromosome == request.Chromosome &&
                    v.Position == request.Position &&
                    v.Alternative == request.Alternative);

            int variantId;
            bool variantWasCreated = false;

            if (existingVariant != null)
            {
                variantId = existingVariant.VariantId;
            }
            else
            {
                var newVariant = new Variant
                {
                    Chromosome = request.Chromosome,
                    Position = request.Position,
                    Alternative = request.Alternative,
                    Reference = request.Reference,
                    UserInfo = request.UserInfo ?? string.Empty
                };
                _context.Variants.Add(newVariant);
                await _context.SaveChangesAsync();
                variantId = newVariant.VariantId;
                variantWasCreated = true;
            }

            // 3. Check existing GeneVariant
            var existingGeneVariant = await _context.GeneVariants
                .FirstOrDefaultAsync(gv => gv.NmId == nmNumber && gv.VariantId == variantId);

            if (existingGeneVariant != null)
                throw new ArgumentException($"This variant is already linked to transcript {nmNumber}");

            // 4. Create GeneVariant directly (like Get method pattern)
            var geneVariant = new GeneVariant
            {
                NmId = nmNumber,
                VariantId = variantId,
                BiologicalEffect = request.BiologicalEffect,
                Classification = request.Classification,
                UserInfo = request.UserInfo ?? string.Empty
            };

            _context.GeneVariants.Add(geneVariant);
            await _context.SaveChangesAsync();

            // 5. Return simple result (like Get method pattern)
            return new AddVariantToTranscriptResult
            {
                VariantWasCreated = variantWasCreated,
                GeneVariant = new GeneVariantResult
                {
                    NmId = geneVariant.NmId,
                    VariantId = geneVariant.VariantId,
                    BiologicalEffect = geneVariant.BiologicalEffect,
                    Classification = geneVariant.Classification,
                    UserInfo = geneVariant.UserInfo
                },
                Message = variantWasCreated
                    ? "New variant created and linked to transcript"
                    : "Existing variant linked to transcript"
            };
        }
    }
}

