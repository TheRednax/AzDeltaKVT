using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzDektaKVT.Model;

namespace AzDeltaKVT.Dto.Requests
{
	public class GeneRequest
    {
        public string Name { get; set; }
        public string Chromosome { get; set; }
        public int Start { get; set; }
        public int Stop { get; set; }
        public string UserInfo { get; set; }

        public string Nm_Number { get; set; }
        public int? Position { get; set; }
    }
}
