using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzDektaKVT.Model;
using Microsoft.EntityFrameworkCore;

namespace AzDeltaKVT.Core
{
	public class AzDeltaKVTDbContext : DbContext
	{
		public AzDeltaKVTDbContext(DbContextOptions<AzDeltaKVTDbContext> options)
			: base(options)
		{
		}
		public DbSet<NmTranscript> NmTranscripts => Set<NmTranscript>();
		public DbSet<Variant> Variants => Set<Variant>();
		public DbSet<Gene> Genes => Set<Gene>();
		public DbSet<GeneVariant> GeneVariants => Set<GeneVariant>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<GeneVariant>(entity =>
			{
				entity.HasKey(e => new { e.NmId, e.VariantId });

				entity.HasOne(e => e.NmTranscript)
					.WithMany()
					.HasForeignKey(e => e.NmId);

				entity.HasOne(e => e.Variant)
					.WithMany()
					.HasForeignKey(e => e.VariantId);
			});
		}
public void Seed()
{
if (Genes.Any()) return;
Genes.AddRange(new List<Gene>
{
	new Gene { Name = "SF3B1", Chromosome = "2", Start = 198254508, Stop = 198299817, UserInfo = "From source example" },
	new Gene { Name = "KIT", Chromosome = "4", Start = 55524124, Stop = 55606881, UserInfo = "From source example" },
	new Gene { Name = "TET2", Chromosome = "4", Start = 106067032, Stop = 106200960, UserInfo = "From source example" },
	new Gene { Name = "DNMT3A", Chromosome = "3", Start = 128200000, Stop = 128300000, UserInfo = "Added for variant coverage" },
	new Gene { Name = "NPM1", Chromosome = "5", Start = 170830000, Stop = 170850000, UserInfo = "Added for variant coverage" },
	new Gene { Name = "EZH2", Chromosome = "7", Start = 148500000, Stop = 148530000, UserInfo = "Added for variant coverage" },
	new Gene { Name = "ASXL1", Chromosome = "2", Start = 31000000, Stop = 31030000, UserInfo = "Added gene" },
	new Gene { Name = "FLT3", Chromosome = "7", Start = 28000000, Stop = 28100000, UserInfo = "Added gene" },
	new Gene { Name = "RUNX1", Chromosome = "7", Start = 36000000, Stop = 36050000, UserInfo = "Added gene" },
	new Gene { Name = "KRAS", Chromosome = "5", Start = 25200000, Stop = 25250000, UserInfo = "Added gene" }
});

NmTranscripts.AddRange(new List<NmTranscript>
{
	new NmTranscript { NmNumber = "NM_012433.3", GeneId = "SF3B1", IsInHouse = true, IsSelect = true, IsClinical = false },
	new NmTranscript { NmNumber = "NM_000222.2", GeneId = "KIT", IsInHouse = true, IsSelect = true, IsClinical = false },
	new NmTranscript { NmNumber = "NM_001093772.1", GeneId = "KIT", IsInHouse = true, IsSelect = false, IsClinical = false },
	new NmTranscript { NmNumber = "NM_001127208.2", GeneId = "TET2", IsInHouse = true, IsSelect = true, IsClinical = true },
	new NmTranscript { NmNumber = "NM_017628.1", GeneId = "TET2", IsInHouse = false, IsSelect = false, IsClinical = false },
	new NmTranscript { NmNumber = "NM_022552.4", GeneId = "DNMT3A", IsInHouse = true, IsSelect = true, IsClinical = false },
	new NmTranscript { NmNumber = "NM_002520.6", GeneId = "NPM1", IsInHouse = true, IsSelect = false, IsClinical = false },
	new NmTranscript { NmNumber = "NM_004456.4", GeneId = "EZH2", IsInHouse = false, IsSelect = false, IsClinical = true },
	new NmTranscript { NmNumber = "NM_015338.5", GeneId = "ASXL1", IsInHouse = true, IsSelect = true, IsClinical = false },
	new NmTranscript { NmNumber = "NM_004119.2", GeneId = "FLT3", IsInHouse = true, IsSelect = false, IsClinical = false }
});

Variants.AddRange(new List<Variant>
{
	new Variant {Chromosome = "2", Position = 198255000, Reference = "G", Alternative = "A", UserInfo = "Example for SF3B1" },
	new Variant {Chromosome = "4", Position = 55525000, Reference = "T", Alternative = "C", UserInfo = "Example for KIT 1" },
	new Variant {Chromosome = "4", Position = 55526000, Reference = "A", Alternative = "G", UserInfo = "Example for KIT 2" },
	new Variant {Chromosome = "4", Position = 106067500, Reference = "C", Alternative = "T", UserInfo = "Example for TET2 1" },
	new Variant {Chromosome = "4", Position = 106068000, Reference = "G", Alternative = "A", UserInfo = "Example for TET2 2" },
	new Variant {Chromosome = "2", Position = 198267369, Reference = "G", Alternative = "A", UserInfo = "Example from the test data" },
	new Variant {Chromosome = "2", Position = 209113048, Reference = "GA", Alternative = "G" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "2", Position = 209113192, Reference = "G", Alternative = "A" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "3", Position = 38181966, Reference = "A", Alternative = "G", UserInfo = "Example from the test data"  },
	new Variant {Chromosome = "3", Position = 128200629, Reference = "A", Alternative = "G" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "4", Position = 55524252, Reference = "G", Alternative = "A" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "4", Position = 55599444, Reference = "A", Alternative = "G" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "4", Position = 106155911, Reference = "C", Alternative = "G" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "4", Position = 106163960, Reference = "ATG", Alternative = "A" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "4", Position = 106196281, Reference = "CCAG", Alternative = "C", UserInfo = "Example from the test data"  },
	new Variant {Chromosome = "5", Position = 170837513, Reference = "CTT", Alternative = "C" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "5", Position = 170837513, Reference = "C", Alternative = "CTT" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "5", Position = 170837513, Reference = "C", Alternative = "CT" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "5", Position = 170837513, Reference = "CTTT", Alternative = "C" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 50358574, Reference = "ATC", Alternative = "A" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 50435616, Reference = "TA", Alternative = "T" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 50435777, Reference = "T", Alternative = "G" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 50444181, Reference = "GT", Alternative = "G" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 50444181, Reference = "G", Alternative = "GT" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 50459588, Reference = "TA", Alternative = "T" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 148504716, Reference = "AG", Alternative = "A" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 148506396, Reference = "A", Alternative = "C" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 148525804, Reference = "T", Alternative = "C" , UserInfo = "Example from the test data" },
	new Variant {Chromosome = "7", Position = 148525888, Reference = "CCAT", Alternative = "C" , UserInfo = "Example from the test data" }
});

			SaveChanges();

			var random = new Random();
			var effects = new[] { "No effect", "Missense mutation", "Nonsense mutation", "Frameshift", "Splice site" };
			var classifications = new[] { "Benign", "Likely benign", "Uncertain significance", "Likely pathogenic", "Pathogenic" };
			var infos = new[] { "ClinVar", "Literature", "In-house", "Hospital data", "To be reviewed" };
			var geneVariants = new List<GeneVariant>();
			for (int i = 0; i < Variants.Count(); i++)
			{
				var variant = Variants.ToList()[i];
				var matchingTranscript = NmTranscripts
					.FirstOrDefault(nm => Genes.Any(g => g.Name == nm.GeneId && g.Chromosome == variant.Chromosome));
				var transcript = matchingTranscript ?? NmTranscripts.ToList()[random.Next(NmTranscripts.Count())];
				geneVariants.Add(new GeneVariant
				{
					NmId = transcript.NmNumber,
					VariantId = i + 1, 
					BiologicalEffect = effects[random.Next(effects.Length)],
					Classification = classifications[random.Next(classifications.Length)],
					UserInfo = infos[random.Next(infos.Length)]
				});
			}
			GeneVariants.AddRange(geneVariants);
			SaveChanges();
		}
	}
}
