using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDeltaKVT.Dto.Results
{
	public class UploadRowResult
	{
		public string Chromosome { get; set; }
		public int Position { get; set; }
		public string Reference { get; set; }
		public string Alternative { get; set; }
		public string? GeneName { get; set; }
		public string? NmNumber { get; set; }
		public bool? IsInHouse { get; set; }
		public string? BiologicalEffect { get; set; }
		public string? Classification { get; set; }
		public bool IsKnownPosition { get; set; }

	}
}
