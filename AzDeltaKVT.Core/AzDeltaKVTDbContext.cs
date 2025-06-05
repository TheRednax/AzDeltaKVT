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

	}
}
