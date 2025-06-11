using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDeltaKVT.Dto.Results
{
    public class AddVariantToTranscriptResult
    {
        public bool VariantWasCreated { get; set; }  
        public GeneVariantResult GeneVariant { get; set; }
        public string Message { get; set; }
    }
}
