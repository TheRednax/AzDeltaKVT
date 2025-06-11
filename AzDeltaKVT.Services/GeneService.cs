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
					.Where(t => t.NmNumber.ToLower() == request.Nm_Number.ToLower())
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
				var nmNumbers = new List<NmTranscript>();
				if (!string.IsNullOrEmpty(request.Chromosome) && request.Position.HasValue)
				{
					// Get associated NM transcripts and sort them
					nmNumbers = await _context.GeneVariants
						.Include(gv => gv.Variant)
						.Include(gv => gv.NmTranscript)
						.Where(t => t.Variant.Chromosome == request.Chromosome && t.Variant.Position == request.Position)
						.Select(gv => gv.NmTranscript)
						.ToListAsync();
				}
				else if (string.IsNullOrEmpty(request.Nm_Number))
				{
					// Get associated NM transcripts and sort them
					nmNumbers = await _context.NmTranscripts
						.Where(t => t.GeneId == gene.Name)
						.ToListAsync();
				}
				else
				{
					nmNumbers = await _context.NmTranscripts
						.Where(t => t.GeneId == gene.Name && t.NmNumber.ToLower() == request.Nm_Number.ToLower()).ToListAsync();
				}

				var orderedNms = nmNumbers
					.OrderByDescending(t => t.IsInHouse)
					.ThenByDescending(t => t.IsSelect)
					.ThenByDescending(t => t.IsClinical)
					.ThenBy(t => t.NmNumber)
					.ToList();

				var variants = new List<Variant>();

				if (string.IsNullOrEmpty(request.Chromosome) && !request.Position.HasValue)
				{
					// Get variants for gene
					variants = await _context.Variants
						.Where(v => v.Chromosome == gene.Chromosome &&
									v.Position >= gene.Start &&
									v.Position <= gene.Stop)
						.ToListAsync();
				}
				else
				{
					// Get variants for gene
					variants = await _context.Variants
						.Where(v => v.Chromosome == request.Chromosome &&
									v.Position == request.Position)
						.ToListAsync();
				}


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
				UserInfo = request.UserInfo ?? string.Empty
			};

			_context.Genes.Add(entity);
			await _context.SaveChangesAsync();

			var result = new GeneResult
			{
				Name = entity.Name,
				Chromosome = entity.Chromosome,
				Start = entity.Start,
				Stop = entity.Stop,
				UserInfo = entity.UserInfo,
				IsSelect = request.IsSelect,
				IsInHouse = request.IsInHouse,
				IsClinical = request.IsClinical
			};

			var transcript = new NmTranscript
			{
				Gene = entity,
				GeneId = entity.Name,
				NmNumber = request.Nm_Number,
				IsSelect = request.IsSelect,
				IsInHouse = request.IsInHouse,
				IsClinical = request.IsClinical
			};

			_context.NmTranscripts.Add(transcript);
			await _context.SaveChangesAsync();

			return result;
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


			var result = new GeneResult
			{
				Name = existing.Name,
				Chromosome = existing.Chromosome,
				Start = existing.Start,
				Stop = existing.Stop,
				UserInfo = existing.UserInfo
			};


			var existingTranscript = await _context.NmTranscripts.FindAsync(gene.Nm_Number);
			if (existingTranscript == null) return null;

			existingTranscript.IsSelect = gene.IsSelect;
			existingTranscript.IsInHouse = gene.IsInHouse;
			existingTranscript.IsClinical = gene.IsClinical;

			await _context.SaveChangesAsync();

			return result;
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

		public async Task<Gene> GetByName(string name)
		{
			return await _context.Genes.FirstOrDefaultAsync(g => g.Name == name);
		}
	}
}