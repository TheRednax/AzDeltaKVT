using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDektaKVT.Model
{
	public class NmTranscript
	{
		[Key]
		public string NmNumber { get; set; }

		public string GeneId { get; set; }  // This should match Gene.Name (FK)
		[ForeignKey(nameof(GeneId))]
		public Gene Gene { get; set; }

		public bool IsSelect { get; set; }
		public bool IsClinical { get; set; }
		public bool IsInHouse { get; set; }
	}

}
