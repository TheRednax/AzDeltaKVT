using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzDektaKVT.Model;

namespace AzDeltaKVT.Dto.Requests
{
	public class GeneVariantRequest
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
