using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzDektaKVT.Model;

namespace AzDeltaKVT.Dto.Requests
{
	public class GeneRequest : Gene
	{
		public string Nm_Number { get; set; }
	}
}
