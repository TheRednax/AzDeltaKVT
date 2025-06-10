using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
using AzDektaKVT.Model;

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

        public async Task<List<GeneResult>> GetAllGenesAsync()
        {
            Console.WriteLine("GetAllGenesAsync called");
            var response = await _httpClient.GetAsync("/genes");
            Console.WriteLine($"GET /genes Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetAllGenes API Response: {json}");
                try
                {
                    var result = JsonSerializer.Deserialize<List<GeneResult>>(json, _jsonOptions) ?? new List<GeneResult>();
                    Console.WriteLine($"Deserialized {result.Count} genes");
                    return result;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON Deserialization error: {ex.Message}");
                    Console.WriteLine($"Raw JSON: {json}");
                    return new List<GeneResult>();
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetAllGenes API Error: {response.StatusCode} - {error}");
            }
            return new List<GeneResult>();
        }

        public async Task<List<GeneResult>> SearchGenesAsync(string? name = null, string? nmNumber = null, int? position = null)
        {
            Console.WriteLine($"SearchGenesAsync called with: name={name}, nmNumber={nmNumber}, position={position}");


            var request = new GeneRequest
            {
                Name = name ?? "",
                Chromosome = "",
                Start = 0,
                Stop = 0,
                UserInfo = null,
                Nm_Number = nmNumber ?? "",
                Position = position
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            Console.WriteLine($"Request JSON: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/genes/get", content);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchGenes API Response: {responseJson}"); // Debug log
                try
                {
                    var result = JsonSerializer.Deserialize<List<GeneResult>>(responseJson, _jsonOptions) ?? new List<GeneResult>();
                    Console.WriteLine($"Deserialized {result.Count} genes");
                    return result;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON Deserialization error: {ex.Message}");
                    Console.WriteLine($"Raw JSON: {responseJson}");
                    return new List<GeneResult>();
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchGenes API Error: {response.StatusCode} - {error}");
            }
            return new List<GeneResult>();
        }

        public async Task<GeneResult?> GetGeneByNameAsync(string name)
        {
            var genes = await SearchGenesAsync(name: name);
            return genes.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }


        public async Task<NmTranscriptResult?> GetTranscriptAsync(string nmNumber)
        {
            var response = await _httpClient.GetAsync($"/transcripts/{Uri.EscapeDataString(nmNumber)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<NmTranscriptResult>(json, _jsonOptions);
            }
            return null;
        }

        // Variant API calls - Updated to use DTOs
        public async Task<List<VariantResult>> GetAllVariantsAsync()
        {
            var response = await _httpClient.GetAsync("/variants");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetAllVariants API Response: {json}"); // Debug log
                return JsonSerializer.Deserialize<List<VariantResult>>(json, _jsonOptions) ?? new List<VariantResult>();
            }
            return new List<VariantResult>();
        }

        public async Task<List<VariantResult>> SearchVariantsAsync(string? chromosome = null, int? position = null, int? variantId = null)
        {
            // Use real VariantRequest DTO
            var request = new VariantRequest
            {
                VariantId = variantId ?? 0,
                Chromosome = chromosome ?? "",
                Position = position ?? 0,
                Alternative = "",
                Reference = "",
                UserInfo = ""
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/variants/get", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchVariants API Response: {responseJson}"); // Debug log
                return JsonSerializer.Deserialize<List<VariantResult>>(responseJson, _jsonOptions) ?? new List<VariantResult>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchVariants API Error: {response.StatusCode} - {error}");
            }
            return new List<VariantResult>();
        }

        public async Task<VariantResult?> GetVariantAsync(int id)
        {
            var variants = await SearchVariantsAsync(variantId: id);
            return variants.FirstOrDefault();
        }

        // Gene Variant API calls - Updated to use DTOs
        public async Task<List<GeneVariantResult>> GetGeneVariantsAsync()
        {
            var response = await _httpClient.GetAsync("/genevariants");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<GeneVariantResult>>(json, _jsonOptions) ?? new List<GeneVariantResult>();
            }
            return new List<GeneVariantResult>();
        }

        // Helper methods for complex operations - updated for DTOs
        public async Task<List<VariantResult>> GetVariantsInGeneRangeAsync(GeneResult gene)
        {
            // Gene already contains variants from the backend response
            return gene.Variants?.Cast<VariantResult>().ToList() ?? new List<VariantResult>();
        }

        public async Task<List<GeneVariantResult>> GetGeneVariantsForVariantsAsync(List<VariantResult> variants)
        {
            var allGeneVariants = await GetGeneVariantsAsync();
            return allGeneVariants.Where(gv =>
                variants.Any(v => v.VariantId == gv.VariantId)).ToList();
        }

        // Convenience method to get transcripts from gene
        public List<NmTranscriptResult> GetTranscriptsFromGene(GeneResult gene)
        {
            return gene.NmNumbers?.Select(t => new NmTranscriptResult
            {
                NmNumber = t.NmNumber ?? "",
                GeneId = t.GeneId ?? "",
                IsSelect = t.IsSelect,
                IsClinical = t.IsClinical,
                IsInHouse = t.IsInHouse
            }).ToList() ?? new List<NmTranscriptResult>();
        }

        // Convenience methods that use the updated endpoints
        public async Task<List<VariantResult>> GetVariantsAsync(string? chromosome = null, int? position = null)
        {
            if (!string.IsNullOrEmpty(chromosome) || position.HasValue)
            {
                return await SearchVariantsAsync(chromosome, position);
            }
            return await GetAllVariantsAsync();
        }

        public async Task<List<GeneResult>> SearchByChromosomePositionAsync(string chromosome, int position)
        {
            Console.WriteLine($"SearchByChromosomePositionAsync called with: chromosome={chromosome}, position={position}");


            var request = new GeneRequest
            {
                Name = "",
                Chromosome = chromosome,
                Start = 0,
                Stop = 0,
                UserInfo = null,
                Nm_Number = "",
                Position = position
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            Console.WriteLine($"Request JSON: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/genes/get", content);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchByChromosomePosition API Response: {responseJson}");
                try
                {
                    var result = JsonSerializer.Deserialize<List<GeneResult>>(responseJson, _jsonOptions) ?? new List<GeneResult>();
                    Console.WriteLine($"Deserialized {result.Count} genes");
                    return result;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON Deserialization error: {ex.Message}");
                    Console.WriteLine($"Raw JSON: {responseJson}");
                    return new List<GeneResult>();
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchByChromosomePosition API Error: {response.StatusCode} - {error}");
            }
            return new List<GeneResult>();
        }
        public async Task<UploadResult> UploadFileAsync(IBrowserFile file)
        {
	        if (file == null)
		        throw new ArgumentNullException(nameof(file));

	        var maxAllowedSize = 10 * 1024 * 1024; // 10 MB max size

	        using var content = new MultipartFormDataContent();
	        using var stream = file.OpenReadStream(maxAllowedSize);
	        var streamContent = new StreamContent(stream);
	        var mediaType = string.IsNullOrWhiteSpace(file.ContentType) ? "text/tab-separated-values" : file.ContentType;
	        streamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);


	        content.Add(streamContent, "TsvFile", file.Name);

	        var response = await _httpClient.PostAsync("/upload", content);

	        if (response.IsSuccessStatusCode)
	        {
		        // Optionally parse response content
		        string result = await response.Content.ReadAsStringAsync();
		        return JsonSerializer.Deserialize<AzDeltaKVT.Dto.Results.UploadResult>(result, _jsonOptions) ?? new AzDeltaKVT.Dto.Results.UploadResult();
	        }
	        else
	        {
		        var error = await response.Content.ReadAsStringAsync();
		        throw new Exception($"File upload failed: {error}");
	        }
        }
    }
}