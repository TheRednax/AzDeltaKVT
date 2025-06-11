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
            var genes = await _context.Genes.OrderBy(g => g.Name).ToListAsync();
            var results = new List<GeneResult>();

            foreach (var gene in genes)
            {
                var transcripts = await GetSortedTranscriptsByGeneId(gene.Name);
                var variants = await GetVariantsForGene(gene);

                results.Add(BuildGeneResult(gene, transcripts, variants));
            }

            return results;
        }

        public async Task<IList<GeneResult>> Get(GeneRequest request)
        {
            var genes = await FilterGenes(request);
            if (!genes.Any()) return new List<GeneResult>();

            var results = new List<GeneResult>();
            foreach (var gene in genes)
            {
                var transcripts = await GetRelevantTranscripts(gene, request);
                var variants = await GetRelevantVariants(gene, request);

                results.Add(BuildGeneResult(gene, transcripts, variants));
            }

            return results;
        }

        public async Task<GeneResult> Create(GeneRequest request)
        {
            var gene = new Gene
            {
                Name = request.Name,
                Chromosome = request.Chromosome,
                Start = request.Start,
                Stop = request.Stop,
                UserInfo = request.UserInfo ?? string.Empty
            };

            _context.Genes.Add(gene);
            await _context.SaveChangesAsync();

            var transcript = new NmTranscript
            {
                GeneId = gene.Name,
                NmNumber = request.Nm_Number,
                IsSelect = request.IsSelect,
                IsInHouse = request.IsInHouse,
                IsClinical = request.IsClinical
            };

            _context.NmTranscripts.Add(transcript);
            await _context.SaveChangesAsync();

            return BuildGeneResult(gene, new List<NmTranscript> { transcript }, new List<Variant>());
        }

        public async Task<GeneResult?> Update(GeneRequest request)
        {
            var gene = await _context.Genes.FindAsync(request.Name);
            if (gene == null) return null;

            gene.Chromosome = request.Chromosome;
            gene.Start = request.Start;
            gene.Stop = request.Stop;
            gene.UserInfo = request.UserInfo;

            var transcript = await _context.NmTranscripts
                .FirstOrDefaultAsync(t => t.GeneId == request.Name && t.NmNumber == request.Nm_Number);

            if (transcript == null) return null;

            transcript.IsSelect = request.IsSelect;
            transcript.IsInHouse = request.IsInHouse;
            transcript.IsClinical = request.IsClinical;

            await _context.SaveChangesAsync();

            return BuildGeneResult(gene, new List<NmTranscript> { transcript }, new List<Variant>());
        }

        public async Task<bool> Delete(string name)
        {
            var gene = await _context.Genes.FindAsync(name);
            if (gene == null) return false;

            _context.Genes.Remove(gene);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Gene?> GetByName(string name)
        {
            return await _context.Genes.FirstOrDefaultAsync(g => g.Name == name);
        }

        // ---------- Private Helpers ----------

        private async Task<IList<Gene>> FilterGenes(GeneRequest request)
        {
            IQueryable<Gene> query = _context.Genes;

            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(g => g.Name.Contains(request.Name));
            }
            else if (!string.IsNullOrEmpty(request.Nm_Number))
            {
                var geneId = await _context.NmTranscripts
                    .Where(t => t.NmNumber.ToLower() == request.Nm_Number.ToLower())
                    .Select(t => t.GeneId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(geneId))
                    return new List<Gene>();

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
                return new List<Gene>();
            }

            return await query.OrderBy(g => g.Name).ToListAsync();
        }

        private async Task<List<NmTranscript>> GetSortedTranscriptsByGeneId(string geneId)
        {
            return await _context.NmTranscripts
                .Where(t => t.GeneId == geneId)
                .OrderByDescending(t => t.IsInHouse)
                .ThenByDescending(t => t.IsSelect)
                .ThenByDescending(t => t.IsClinical)
                .ThenBy(t => t.NmNumber)
                .ToListAsync();
        }

        private async Task<List<NmTranscript>> GetRelevantTranscripts(Gene gene, GeneRequest request)
        {
            if (!string.IsNullOrEmpty(request.Chromosome) && request.Position.HasValue)
            {
                return await _context.GeneVariants
                    .Include(gv => gv.NmTranscript)
                    .Where(gv => gv.Variant.Chromosome == request.Chromosome &&
                                 gv.Variant.Position == request.Position)
                    .Select(gv => gv.NmTranscript)
                    .ToListAsync();
            }

            if (!string.IsNullOrEmpty(request.Nm_Number))
            {
                return await _context.NmTranscripts
                    .Where(t => t.GeneId == gene.Name && t.NmNumber.ToLower() == request.Nm_Number.ToLower())
                    .ToListAsync();
            }

            return await GetSortedTranscriptsByGeneId(gene.Name);
        }

        private async Task<List<Variant>> GetVariantsForGene(Gene gene)
        {
            return await _context.Variants
                .Where(v => v.Chromosome == gene.Chromosome &&
                            v.Position >= gene.Start &&
                            v.Position <= gene.Stop)
                .ToListAsync();
        }

        private async Task<List<Variant>> GetRelevantVariants(Gene gene, GeneRequest request)
        {
            if (!string.IsNullOrEmpty(request.Chromosome) && request.Position.HasValue)
            {
                return await _context.Variants
                    .Where(v => v.Chromosome == request.Chromosome &&
                                v.Position == request.Position)
                    .ToListAsync();
            }

            return await GetVariantsForGene(gene);
        }

        private GeneResult BuildGeneResult(Gene gene, List<NmTranscript> transcripts, List<Variant> variants)
        {
            var orderedTranscripts = transcripts
                .OrderByDescending(t => t.IsInHouse)
                .ThenByDescending(t => t.IsSelect)
                .ThenByDescending(t => t.IsClinical)
                .ThenBy(t => t.NmNumber)
                .ToList();

            return new GeneResult
            {
                Name = gene.Name,
                Chromosome = gene.Chromosome,
                Start = gene.Start,
                Stop = gene.Stop,
                UserInfo = gene.UserInfo,
                NmNumbers = orderedTranscripts,
                Variants = variants
            };
        }
    }
}