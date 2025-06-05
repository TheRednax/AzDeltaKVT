using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDektaKVT.Model
{
	public class GeneVariant
	{
		[Key]
		public string NmId { get; set; }
		[ForeignKey(nameof(NmId))]
		public NmTranscript NmTranscript { get; set; }
		[Key]
		public int VariantId { get; set; }
		[ForeignKey(nameof(VariantId))]
		public Variant Variant { get; set; }

		public string BiologicalEffect { get; set; }
		public string Classification { get; set; }
		public string UserInfo { get; set; }
	}

}
