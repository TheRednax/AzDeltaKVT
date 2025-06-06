using System.Text.Json;

namespace AzDeltaKVT.UI.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Gene API calls
        public async Task<List<GeneModel>> GetGenesAsync(string? name = null)
        {
            var url = "/genes";
            if (!string.IsNullOrEmpty(name))
            {
                url += $"?name={Uri.EscapeDataString(name)}";
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<GeneModel>>(json, _jsonOptions) ?? new List<GeneModel>();
            }
            return new List<GeneModel>();
        }

        public async Task<GeneModel?> GetGeneAsync(string name)
        {
            var response = await _httpClient.GetAsync($"/genes/{Uri.EscapeDataString(name)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GeneModel>(json, _jsonOptions);
            }
            return null;
        }

        // Transcript API calls
        public async Task<TranscriptModel?> GetTranscriptAsync(string nmNumber)
        {
            var response = await _httpClient.GetAsync($"/transcripts/{Uri.EscapeDataString(nmNumber)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TranscriptModel>(json, _jsonOptions);
            }
            return null;
        }

        // Variant API calls
        public async Task<List<VariantModel>> GetVariantsAsync(string? chromosome = null, int? position = null)
        {
            var url = "/variants";
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(chromosome))
            {
                queryParams.Add($"chrom={Uri.EscapeDataString(chromosome)}");
            }
            if (position.HasValue)
            {
                queryParams.Add($"position={position.Value}");
            }

            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<VariantModel>>(json, _jsonOptions) ?? new List<VariantModel>();
            }
            return new List<VariantModel>();
        }

        public async Task<VariantModel?> GetVariantAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/variants/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<VariantModel>(json, _jsonOptions);
            }
            return null;
        }

        // Gene Variant API calls
        public async Task<List<GeneVariantModel>> GetGeneVariantsAsync()
        {
            var response = await _httpClient.GetAsync("/genevariants");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<GeneVariantModel>>(json, _jsonOptions) ?? new List<GeneVariantModel>();
            }
            return new List<GeneVariantModel>();
        }

        public async Task<GeneVariantModel?> GetGeneVariantAsync(string nmId, int variantId)
        {
            var response = await _httpClient.GetAsync($"/genevariants/{Uri.EscapeDataString(nmId)}/{variantId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GeneVariantModel>(json, _jsonOptions);
            }
            return null;
        }

        // Helper method to get variants within a gene range
        public async Task<List<VariantModel>> GetVariantsInGeneRangeAsync(GeneModel gene)
        {
            var allVariants = await GetVariantsAsync(gene.Chromosome);
            return allVariants.Where(v =>
                v.Position >= gene.Start &&
                v.Position <= gene.Stop).ToList();
        }

        // Helper method to get gene variants for specific variants
        public async Task<List<GeneVariantModel>> GetGeneVariantsForVariantsAsync(List<VariantModel> variants)
        {
            var allGeneVariants = await GetGeneVariantsAsync();
            return allGeneVariants.Where(gv =>
                variants.Any(v => v.VariantId == gv.VariantId)).ToList();
        }
    }

    // Model classes
    public class GeneModel
    {
        public string Name { get; set; } = "";
        public string Chromosome { get; set; } = "";
        public int Start { get; set; }
        public int Stop { get; set; }
        public string? UserInfo { get; set; }
    }

    public class VariantModel
    {
        public int VariantId { get; set; }
        public string Chromosome { get; set; } = "";
        public int Position { get; set; }
        public string Reference { get; set; } = "";
        public string Alternative { get; set; } = "";
        public string? UserInfo { get; set; }
    }

    public class TranscriptModel
    {
        public string NmNumber { get; set; } = "";
        public string GeneId { get; set; } = "";
        public bool IsSelect { get; set; }
        public bool IsClinical { get; set; }
        public bool IsInHouse { get; set; }
        public GeneModel? Gene { get; set; }
    }

    public class GeneVariantModel
    {
        public string NmId { get; set; } = "";
        public int VariantId { get; set; }
        public string? BiologicalEffect { get; set; }
        public string? Classification { get; set; }
        public string? UserInfo { get; set; }
    }
}
