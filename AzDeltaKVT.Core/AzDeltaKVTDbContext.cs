using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzDektaKVT.Model;
using Microsoft.EntityFrameworkCore;

namespace AzDeltaKVT.Core
{
	public class AzDeltaKVTDbContext(DbContextOptions<AzDeltaKVTDbContext> options) : DbContext
	{
		public DbSet<NmTranscript> NmTranscripts => Set<NmTranscript>();
		public DbSet<Variant> Variants => Set<Variant>();
		public DbSet<Gene> Genes => Set<Gene>();
		public DbSet<GeneVariant> GeneVariants => Set<GeneVariant>();

	}
}
