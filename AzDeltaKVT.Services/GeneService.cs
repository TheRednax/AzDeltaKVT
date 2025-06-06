using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
using Microsoft.EntityFrameworkCore;

namespace AzDeltaKVT.Services
{
    public class GeneService
    {
        private readonly AzDeltaKVTDbContext _context;

        public GeneService(AzDeltaKVTDbContext context)
        {
            _context = context;
        }

        public async Task<IList<GeneResult>> Find()
        {
            var genes = await _context.Genes
                .OrderBy(g => g.Name) // Sort genes alphabetically
                .ToListAsync();

            var results = new List<GeneResult>();

            foreach (var gene in genes)
            {
                // Fetch related transcripts
                var nmTranscripts = await _context.NmTranscripts
                    .Where(t => t.GeneId == gene.Name)
                    .ToListAsync();

                // Sort transcripts: InHouse > Select > Clinical > Alphabetical
                var sortedTranscripts = nmTranscripts
                    .OrderByDescending(t => t.IsInHouse)
                    .ThenByDescending(t => t.IsSelect)
                    .ThenByDescending(t => t.IsClinical)
                    .ThenBy(t => t.NmNumber)
                    .ToList();

                results.Add(new GeneResult
                {
                    Name = gene.Name,
                    Chromosome = gene.Chromosome,
                    Start = gene.Start,
                    Stop = gene.Stop,
                    UserInfo = gene.UserInfo,
                    NmNumbers = sortedTranscripts
                });
            }

            return results;
        }

        public async Task<IList<GeneResult>> Get(GeneRequest request)
        {
            IQueryable<Gene> query = _context.Genes;

            // Apply filters
            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(g => g.Name.Contains(request.Name));
            }
            else if (!string.IsNullOrEmpty(request.Nm_Number))
            {
                var geneId = await _context.NmTranscripts
                    .Where(t => t.NmNumber == request.Nm_Number)
                    .Select(t => t.GeneId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(geneId))
                    return new List<GeneResult>();

                query = query.Where(g => g.Name == geneId);
            }
            else if (!string.IsNullOrEmpty(request.Chromosome) && request.Position.HasValue)
            {
                query = query.Where(g =>
                    g.Chromosome == request.Chromosome &&
                    g.Start <= request.Position &&
                    g.Stop >= request.Position);
            }
            else
            {
                return new List<GeneResult>();
            }

            var genes = await query
                .OrderBy(g => g.Name)
                .ToListAsync();

            var results = new List<GeneResult>();

            foreach (var gene in genes)
            {
                // Get associated NM transcripts and sort them
                var nmNumbers = await _context.NmTranscripts
                    .Where(t => t.GeneId == gene.Name)
                    .ToListAsync();

                var orderedNms = nmNumbers
                    .OrderByDescending(t => t.IsInHouse)
                    .ThenByDescending(t => t.IsSelect)
                    .ThenByDescending(t => t.IsClinical)
                    .ThenBy(t => t.NmNumber)
                    .ToList();

                // Get variants for gene
                var variants = await _context.Variants
                    .Where(v => v.Chromosome == gene.Chromosome &&
                                v.Position >= gene.Start &&
                                v.Position <= gene.Stop)
                    .ToListAsync();

                // Build result
                results.Add(new GeneResult
                {
                    Name = gene.Name,
                    Chromosome = gene.Chromosome,
                    Start = gene.Start,
                    Stop = gene.Stop,
                    UserInfo = gene.UserInfo,
                    NmNumbers = orderedNms,
                    Variants = variants
                });
            }

            return results;
        }

        public async Task<GeneResult> Create(GeneRequest request)
        {
            var entity = new Gene
            {
                Name = request.Name,
                Chromosome = request.Chromosome,
                Start = 0,
                Stop = request.Stop,
                UserInfo = request.UserInfo
            };

            _context.Genes.Add(entity);
            await _context.SaveChangesAsync();

            return new GeneResult
            {
                Name = entity.Name,
                Chromosome = entity.Chromosome,
                Start = entity.Start,
                Stop = entity.Stop,
                UserInfo = entity.UserInfo
            };
        }

        public async Task<GeneResult> Update(GeneRequest gene)
        {
            var existing = await _context.Genes.FindAsync(gene.Name);
            if (existing == null) return null;

            existing.Chromosome = gene.Chromosome;
            existing.Start = gene.Start;
            existing.Stop = gene.Stop;
            existing.UserInfo = gene.UserInfo;

            await _context.SaveChangesAsync();

            return new GeneResult
            {
                Name = existing.Name,
                Chromosome = existing.Chromosome,
                Start = existing.Start,
                Stop = existing.Stop,
                UserInfo = existing.UserInfo
            };
        }

        public async Task<bool> Delete(string name)
        {
            var gene = await _context.Genes.FindAsync(name);
            if (gene == null)
                return false;

            _context.Genes.Remove(gene);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}