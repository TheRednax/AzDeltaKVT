using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDeltaKVT.Dto.Requests
{
    public class AddVariantToTranscriptRequest
    {
        [Required]
        public string Chromosome { get; set; }

        [Required]
        public int Position { get; set; }

        [Required]
        public string Alternative { get; set; }

        public string? Reference { get; set; }
        public string? BiologicalEffect { get; set; }
        public string? Classification { get; set; }
        public string? UserInfo { get; set; }
    }
}
