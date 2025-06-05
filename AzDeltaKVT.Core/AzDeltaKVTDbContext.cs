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
					new Gene { Name = "TET2", Chromosome = "4", Start = 106067032, Stop = 106200960, UserInfo = "From source example" }
				});

			NmTranscripts.AddRange(new List<NmTranscript>
				{
					new NmTranscript { NmNumber = "NM_012433.3", GeneId = "SF3B1", IsInHouse = true, IsSelect = true, IsClinical = false },
					new NmTranscript { NmNumber = "NM_000222.2", GeneId = "KIT", IsInHouse = true, IsSelect = true, IsClinical = false },
					new NmTranscript { NmNumber = "NM_001093772.1", GeneId = "KIT", IsInHouse = true, IsSelect = false, IsClinical = false },
					new NmTranscript { NmNumber = "NM_001127208.2", GeneId = "TET2", IsInHouse = true, IsSelect = true, IsClinical = true },
					new NmTranscript { NmNumber = "NM_017628.1", GeneId = "TET2", IsInHouse = false, IsSelect = false, IsClinical = false }
				});

			Variants.AddRange(new List<Variant>
				{
					new Variant {Chromosome = "2", Position = 198255000, Reference = "G", Alternative = "A", UserInfo = "Example for SF3B1" },
					new Variant {Chromosome = "4", Position = 55525000, Reference = "T", Alternative = "C", UserInfo = "Example for KIT 1" },
					new Variant {Chromosome = "4", Position = 55526000, Reference = "A", Alternative = "G", UserInfo = "Example for KIT 2" },
					new Variant {Chromosome = "4", Position = 106067500, Reference = "C", Alternative = "T", UserInfo = "Example for TET2 1" },
					new Variant {Chromosome = "4", Position = 106068000, Reference = "G", Alternative = "A", UserInfo = "Example for TET2 2" }
				});

			GeneVariants.AddRange(new List<GeneVariant>
				{
					new GeneVariant { NmId = "NM_012433.3", VariantId = 1, BiologicalEffect = "Gewijzigde proteïne", Classification = "Onbekend", UserInfo = "ClinVar suggestie" },
					new GeneVariant { NmId = "NM_000222.2", VariantId = 2, BiologicalEffect = "Geen effect", Classification = "Niet pathogeen", UserInfo = "Literatuur" },
					new GeneVariant { NmId = "NM_001093772.1", VariantId = 3, BiologicalEffect = "Gewijzigde proteïne", Classification = "Pathogeen", UserInfo = "In-house" },
					new GeneVariant { NmId = "NM_001127208.2", VariantId = 4, BiologicalEffect = "Geen proteïne", Classification = "Pathogeen", UserInfo = "Ziekenhuis" },
					new GeneVariant { NmId = "NM_017628.1", VariantId = 5, BiologicalEffect = "Gewijzigde proteïne", Classification = "Onbekend", UserInfo = "Nog te beoordelen" }
				});

			SaveChanges();
		}


	}
}
