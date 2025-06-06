using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzDektaKVT.Model;

namespace AzDeltaKVT.Dto.Requests
{
	public class VariantRequest
    {
        public int VariantId { get; set; }
        public string Chromosome { get; set; }
        public int? Position { get; set; }
        public string? Alternative { get; set; }

        public string? Reference { get; set; }
        public string? UserInfo { get; set; }
    }
}
