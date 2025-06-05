using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDektaKVT.Model
{
	public class Variant
	{
		[Key]
		public int VariantId { get; set; }
		public string Chromosome { get; set; }
		public int Position { get; set; }
		public string Alternative { get; set; }

		public string Reference { get; set; }
		public string UserInfo { get; set; }
	}

}
