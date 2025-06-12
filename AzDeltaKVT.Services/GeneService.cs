using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
using Azure.Core;
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

        // Retrieves all genes with their sorted transcripts and associated variants.
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

        //Retrieves genes filtered by the given request parameters, along with relevant transcripts and variants.
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

        //Creates a new gene and its associated transcript using the provided request data.
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

        //Updates an existing gene and its transcript based on the request data. Returns null if not found.
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

        // Deletes the gene with the specified name. Returns false if the gene does not exist.
        public async Task<bool> Delete(string name)
        {
            var gene = await _context.Genes.FindAsync(name);
            if (gene == null) return false;

            _context.Genes.Remove(gene);
            await _context.SaveChangesAsync();
            return true;
        }

        // Retrieves a single gene by its name
        public async Task<Gene?> GetByName(string name)
        {
            return await _context.Genes.FirstOrDefaultAsync(g => g.Name.ToLower() == name.ToLower());
        }

        // Filters genes based on criteria in the request (e.g., name, NM number, chromosome/position).
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

        // Retrieves all transcripts for a given gene ID, sorted by in-house, select, clinical, and NM number.
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

        // Retrieves relevant transcripts for a gene based on the request (position, NM number).
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

        // Retrieves all variants associated with a given gene.
        private async Task<List<Variant>> GetVariantsForGene(Gene gene)
        {
            var nmTranscripts = await _context.NmTranscripts.Where(t => t.GeneId == gene.Name).ToListAsync();
            var variants = new List<Variant>();
			foreach (var nmTranscript in nmTranscripts)
            {
	            var var = await _context.GeneVariants.Include(gv => gv.Variant)
                    .Where(gv => gv.NmId == nmTranscript.NmNumber)
                    .Select(gv => gv.Variant)
                    .ToListAsync();

	            variants.AddRange(var);
			}

			return variants;
        }

        // Retrieves variants relevant to the gene based on request.
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

        // Constructs a GeneResult object from a gene, its transcripts, and its variants.
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