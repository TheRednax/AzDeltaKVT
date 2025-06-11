using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzDektaKVT.Model;

namespace AzDeltaKVT.Dto.Results
{
    public class NmTranscriptResult
    {
        public string NmNumber { get; set; }

        public string GeneId { get; set; }  // This should match Gene.Name (FK)
        [ForeignKey(nameof(GeneId))]
        public Gene Gene { get; set; }

        public bool IsSelect { get; set; }
        public bool IsClinical { get; set; }
        public bool IsInHouse { get; set; }
        public List<GeneVariantResult>? GeneVariants { get; set; } = new();
    }
}
