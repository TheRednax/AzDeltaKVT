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
		public string NmId { get; set; }
		public NmTranscript NmTranscript { get; set; }

		public int VariantId { get; set; }
		public Variant Variant { get; set; }

		public string BiologicalEffect { get; set; }
		public string Classification { get; set; }
		public string UserInfo { get; set; }
	}


}
