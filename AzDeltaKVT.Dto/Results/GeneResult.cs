using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzDektaKVT.Model;

namespace AzDeltaKVT.Dto.Results
{
	public class GeneResult
    {
        public string Name { get; set; }
        public string Chromosome { get; set; }
        public int Start { get; set; }
        public int Stop { get; set; }
        public string UserInfo { get; set; }

        public List<NmTranscript> NmNumbers { get; set; } = new();
        public List<Variant> Variants { get; set; } = new();
        public bool IsSelect { get; set; }
        public bool IsClinical { get; set; }
        public bool IsInHouse { get; set; }
    }
}
