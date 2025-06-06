using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;

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

        // Gene API calls - Updated for new controller structure
        public async Task<List<Gene>> GetAllGenesAsync()
        {
            var response = await _httpClient.GetAsync("/genes");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetAllGenes API Response: {json}"); // Debug log
                return JsonSerializer.Deserialize<List<Gene>>(json, _jsonOptions) ?? new List<Gene>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetAllGenes API Error: {response.StatusCode} - {error}");
            }
            return new List<Gene>();
        }

        public async Task<List<Gene>> SearchGenesAsync(string? name = null, string? nmNumber = null, int? position = null)
        {
            var request = new
            {
                Name = name ?? "",
                Chromosome = "",
                Start = 0,
                Stop = 0,
                UserInfo = (string?)null,
                Nm_Number = nmNumber ?? "",
                Position = position
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/genes/get", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchGenes API Response: {responseJson}"); // Debug log
                return JsonSerializer.Deserialize<List<Gene>>(responseJson, _jsonOptions) ?? new List<Gene>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchGenes API Error: {response.StatusCode} - {error}");
            }
            return new List<Gene>();
        }

        public async Task<Gene?> GetGeneByNameAsync(string name)
        {
            var genes = await SearchGenesAsync(name: name);
            return genes.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        // Transcript API calls - Keep same structure (not changed)
        public async Task<NmTranscript?> GetTranscriptAsync(string nmNumber)
        {
            var response = await _httpClient.GetAsync($"/transcripts/{Uri.EscapeDataString(nmNumber)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<NmTranscript>(json, _jsonOptions);
            }
            return null;
        }

        // Variant API calls - Updated for new controller structure
        public async Task<List<Variant>> GetAllVariantsAsync()
        {
            var response = await _httpClient.GetAsync("/variants");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetAllVariants API Response: {json}"); // Debug log
                return JsonSerializer.Deserialize<List<Variant>>(json, _jsonOptions) ?? new List<Variant>();
            }
            return new List<Variant>();
        }

        public async Task<List<Variant>> SearchVariantsAsync(string? chromosome = null, int? position = null, int? variantId = null)
        {
            var request = new
            {
                VariantId = variantId ?? 0,
                Chromosome = chromosome ?? "",
                Position = position ?? 0,
                Alternative = "",
                Reference = "",
                UserInfo = (string?)null
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/variants/get", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchVariants API Response: {responseJson}"); // Debug log
                return JsonSerializer.Deserialize<List<Variant>>(responseJson, _jsonOptions) ?? new List<Variant>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchVariants API Error: {response.StatusCode} - {error}");
            }
            return new List<Variant>();
        }

        public async Task<Variant?> GetVariantAsync(int id)
        {
            var variants = await SearchVariantsAsync(variantId: id);
            return variants.FirstOrDefault();
        }

        // Gene Variant API calls - Keep same structure (not changed)
        public async Task<List<GeneVariant>> GetGeneVariantsAsync()
        {
            var response = await _httpClient.GetAsync("/genevariants");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<GeneVariant>>(json, _jsonOptions) ?? new List<GeneVariant>();
            }
            return new List<GeneVariant>();
        }

        // Helper methods for complex operations
        public async Task<List<Variant>> GetVariantsInGeneRangeAsync(Gene gene)
        {
            // Get all variants on same chromosome, then filter by range
            var allVariants = await SearchVariantsAsync(chromosome: gene.Chromosome);
            return allVariants.Where(v =>
                v.Position >= gene.Start &&
                v.Position <= gene.Stop).ToList();
        }

        public async Task<List<GeneVariant>> GetGeneVariantsForVariantsAsync(List<Variant> variants)
        {
            var allGeneVariants = await GetGeneVariantsAsync();
            return allGeneVariants.Where(gv =>
                variants.Any(v => v.VariantId == gv.VariantId)).ToList();
        }

        // Convenience methods that use the updated endpoints
        public async Task<List<Variant>> GetVariantsAsync(string? chromosome = null, int? position = null)
        {
            if (!string.IsNullOrEmpty(chromosome) || position.HasValue)
            {
                return await SearchVariantsAsync(chromosome, position);
            }
            return await GetAllVariantsAsync();
        }
    }

    // Simple frontend model classes (only what we need for UI)
    public class Gene
    {
        public string Name { get; set; } = "";
        public string Chromosome { get; set; } = "";
        public int Start { get; set; }
        public int Stop { get; set; }
        public string? UserInfo { get; set; }
    }

    public class Variant
    {
        public int VariantId { get; set; }
        public string Chromosome { get; set; } = "";
        public int Position { get; set; }
        public string Reference { get; set; } = "";
        public string Alternative { get; set; } = "";
        public string? UserInfo { get; set; }
    }

    public class NmTranscript
    {
        public string NmNumber { get; set; } = "";
        public string GeneId { get; set; } = "";
        public bool IsSelect { get; set; }
        public bool IsClinical { get; set; }
        public bool IsInHouse { get; set; }
        public Gene? Gene { get; set; }
    }

    public class GeneVariant
    {
        public string NmId { get; set; } = "";
        public int VariantId { get; set; }
        public string? BiologicalEffect { get; set; }
        public string? Classification { get; set; }
        public string? UserInfo { get; set; }
    }
}