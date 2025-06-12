using AzDektaKVT.Model;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

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


        // Get all genes
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

        // Search genes by name, nmNumber or position
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

        // Method to get a gene by name
        public async Task<GeneResult?> GetGeneByNameAsync(string name)
        {
            var genes = await SearchGenesAsync(name: name);
            return genes.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }


        // Method to get a gene by NM number
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

        // Method to get all variants and gene variants

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


        // Method to get all gene variants
        public async Task<List<GeneVariantResult>> GetAllGeneVariantsAsync()
        {
            var response = await _httpClient.GetAsync("/genevariants");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetAllGeneVariants API Response: {json}"); // Debug log
                return JsonSerializer.Deserialize<List<GeneVariantResult>>(json, _jsonOptions) ?? new List<GeneVariantResult>();
            }
            return new List<GeneVariantResult>();
        }

        // Method to get positions from NmId
        public async Task<List<GeneVariantResult>> GetPositionsFromNm(string? Nm = null)
        {
            var allVariants = await GetAllGeneVariantsAsync();

            var filtered = allVariants
                .Where(gv => string.IsNullOrEmpty(Nm) || gv.NmId == Nm)
                .ToList();

            Console.WriteLine($"Filtered GeneVariants for Nm '{Nm}':");

            foreach (var gv in filtered)
            {
                Console.WriteLine($"- NmId: {gv.NmId}, VariantId: {gv.VariantId}");
            }

            return filtered;
        }

        // Method to search variants by chromosome, position or variantId
        public async Task<List<VariantResult>> SearchVariantsAsync(string? chromosome = null, int? position = null, int? variantId = null)
        {
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
                Console.WriteLine($"SearchVariants API Response: {responseJson}");

                try
                {
                    // Try to deserialize as a single object first
                    var singleVariant = JsonSerializer.Deserialize<VariantResult>(responseJson, _jsonOptions);
                    if (singleVariant != null)
                    {
                        return new List<VariantResult> { singleVariant };
                    }
                }
                catch (JsonException)
                {
                    try
                    {
                        var variantList = JsonSerializer.Deserialize<List<VariantResult>>(responseJson, _jsonOptions);
                        return variantList ?? new List<VariantResult>();
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON Deserialization error for both single and array: {ex.Message}");
                        Console.WriteLine($"Raw JSON: {responseJson}");
                    }
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SearchVariants API Error: {response.StatusCode} - {error}");
            }
            return new List<VariantResult>();
        }

        // Method to get all gene variants
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

        // Method to get gene variants for a list of variants
        public async Task<List<GeneVariantResult>> GetGeneVariantsForVariantsAsync(List<VariantResult> variants)
        {
            var allGeneVariants = await GetGeneVariantsAsync();
            var foundVariants = allGeneVariants.Where(gv =>
                variants.Any(v => v.VariantId == gv.VariantId)).ToList();
            return foundVariants;
        }

        // Method to get transcripts from a gene
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

        // Method to search genes by chromosome and position
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

        // Method to upload a file
        public async Task<UploadResult> UploadFileAsync(IBrowserFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var maxAllowedSize = 10 * 1024 * 1024; 

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream(maxAllowedSize);
            var streamContent = new StreamContent(stream);
            var mediaType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "text/tab-separated-values"
                : file.ContentType;
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);


            content.Add(streamContent, "TsvFile", file.Name);

            var response = await _httpClient.PostAsync("/upload", content);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AzDeltaKVT.Dto.Results.UploadResult>(result, _jsonOptions) ??
                       new AzDeltaKVT.Dto.Results.UploadResult();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"File upload failed: {error}");
            }
        }

        // Method to create a new gene
        public async Task<bool> CreateGeneAsync(GeneRequest request)
        {
            Console.WriteLine($"CreateGeneAsync called with: {request.Name}");

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            Console.WriteLine($"Create Gene Request JSON: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/genes/create", content);
            Console.WriteLine($"Create Gene Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Gene created successfully");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Create Gene API Error: {response.StatusCode} - {error}");

                if (error.Contains("Transcript already exists"))
                {
                    throw new Exception("Failed to create gene: transcript already exists, please choose a new transcript number");
                }
                else if (error.Contains("combination of Transcript number and Gene name"))
                {
                    throw new Exception("Failed to create gene: This combination of Transcript number and Gene name already exists,please choose a new transcript number or gene name");
                }
                else
                {
                    throw new Exception($"Failed to create gene: {error}");
                }
            }
        }

        // Method to update an existing gene
        public async Task<bool> UpdateGeneAsync(GeneRequest request)
        {
            Console.WriteLine($"UpdateGeneAsync called with: {request.Name}");

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            Console.WriteLine($"Update Gene Request JSON: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("/genes/update", content);
            Console.WriteLine($"Update Gene Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Gene updated successfully");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Update Gene API Error: {response.StatusCode} - {error}");
                throw new Exception($"Failed to update gene: {error}");
            }
        }

        // Method to remove a gene by name
        public async Task<bool> RemoveGeneAsync(string geneName)
        {
            Console.WriteLine($"RemoveGeneAsync called with: {geneName}");

            var json = JsonSerializer.Serialize(geneName, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Delete, "/genes/delete")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            Console.WriteLine($"Remove Gene Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Gene removed successfully");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Remove Gene API Error: {response.StatusCode} - {error}");
                throw new Exception($"Failed to remove gene: {error}");
            }
        }

        //  Method to remove a transcript by NM number
        public async Task<bool> RemoveTranscriptAsync(string nmNumber)
        {
            Console.WriteLine($"RemoveTranscriptAsync called with: {nmNumber}");

            var route = $"/Transcripts/{nmNumber}";
            var response = await _httpClient.DeleteAsync(route);
            Console.WriteLine($"Remove Transcript Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Transcript removed successfully");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Remove Transcript API Error: {response.StatusCode} - {error}");
                throw new Exception($"Failed to remove transcript: {error}");
            }
        }

        // Method to update a position (gene variant)
        public async Task<bool> UpdatePosition(GeneVariantRequest request)
        {
            Console.WriteLine($"UpdatePosition called with: NmId={request.NmId}, VariantId={request.VariantId}");

            var genes = await SearchGenesAsync(nmNumber: request.NmId);
            if (genes == null || !genes.Any())
            {
                throw new Exception($"Geen gen gevonden voor NM number {request.NmId}");
            }

            var gene = genes.First();
            var transcripts = GetTranscriptsFromGene(gene);
            var selectedTranscript = transcripts.FirstOrDefault(t => t.NmNumber == request.NmId);

            if (selectedTranscript == null)
            {
                throw new Exception($"Geen transcript gevonden voor NM number {request.NmId}");
            }

            request.NmTranscript = new NmTranscript
            {
                NmNumber = selectedTranscript.NmNumber,
                GeneId = selectedTranscript.GeneId,
                IsSelect = selectedTranscript.IsSelect,
                IsClinical = selectedTranscript.IsClinical,
                IsInHouse = selectedTranscript.IsInHouse,
                Gene = new Gene
                {
                    Name = gene.Name,
                    Chromosome = gene.Chromosome,
                    Start = gene.Start,
                    Stop = gene.Stop,
                    UserInfo = gene.UserInfo
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            Console.WriteLine($"Update Position Request JSON: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("/genevariants/update", content);
            Console.WriteLine($"Update Position Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Position updated successfully");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Update Position API Error: {response.StatusCode} - {error}");
                throw new Exception($"Failed to update position: {error}");
            }
        }

        // Method to create a new position (gene variant)
        public async Task<bool> CreatePosition(GeneVariantRequest request)
        {
            Console.WriteLine($"CreatePosition called with: NmId={request.NmId}, VariantId={request.VariantId}");

            var genes = await SearchGenesAsync(nmNumber: request.NmId);
            if (genes == null || !genes.Any())
            {
                throw new Exception($"Geen gen gevonden voor NM number {request.NmId}");
            }

            var gene = genes.First();

            var transcripts = GetTranscriptsFromGene(gene);
            var selectedTranscript = transcripts.FirstOrDefault(t => t.NmNumber == request.NmId);
            if (selectedTranscript == null)
            {
                throw new Exception($"Geen transcript gevonden voor NM number {request.NmId}");
            }

            var transcriptModel = new NmTranscript
            {
                NmNumber = selectedTranscript.NmNumber,
                GeneId = selectedTranscript.GeneId,
                IsSelect = selectedTranscript.IsSelect,
                IsClinical = selectedTranscript.IsClinical,
                IsInHouse = selectedTranscript.IsInHouse,
                Gene = new Gene
                {
                    Name = gene.Name,
                    Chromosome = gene.Chromosome,
                    Start = gene.Start,
                    Stop = gene.Stop,
                    UserInfo = gene.UserInfo
                }
            };

            request.NmTranscript = transcriptModel;

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            Console.WriteLine($"Create Position Request JSON: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/genevariants/create", content);

            Console.WriteLine($"Create Position Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Position succesvol aangemaakt");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {response.StatusCode} - {error}");
                throw new Exception($"Aanmaken mislukt: {error}");
            }
        }

        // Method to get position by variant ID
        public async Task<GeneVariantResult> GetPositionByVariantId(int variantId)
        {
            var requestData = new GeneVariantRequest
            {
                NmId = "NM_001230",
                NmTranscript = new NmTranscript
                {
                    NmNumber = "NM_001230",
                    GeneId = "DAB",
                    Gene = new Gene
                    {
                        Name = "DAB",
                        Chromosome = "1",
                        Start = 1000,
                        Stop = 2000,
                        UserInfo = "Test gene info"
                    },
                    IsSelect = true,
                    IsClinical = true,
                    IsInHouse = true
                },
                VariantId = variantId,
                Variant = new Variant
                {
                    Chromosome = "",
                    Position = 0,
                    Reference = "",
                    Alternative = "",
                    UserInfo = ""
                },
                BiologicalEffect = "",
                Classification = "",
                UserInfo = ""
            };



            var jsonRequest = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Console.WriteLine($"GetPositionByVariantId POST called with: VariantId={variantId}");
            var response = await _httpClient.PostAsync("/genevariants/get", content);
            Console.WriteLine($"Get Position Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Get Position API Response: {json}");
                return JsonSerializer.Deserialize<GeneVariantResult>(json, _jsonOptions);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Get Position API Error: {response.StatusCode} - {error}");
                throw new Exception($"Failed to get position: {error}");
            }
        }

        // Method to delete a position (gene variant)
        public async Task<bool> DeletePosition(int? variantId)
        {
            if (variantId == null)
                throw new ArgumentNullException(nameof(variantId), "variantId mag niet null zijn.");

            var response = await _httpClient.DeleteAsync($"/genevariants/delete/{variantId}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Variant {variantId} succesvol verwijderd.");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Fout bij verwijderen: {response.StatusCode} - {error}");
                throw new Exception($"Verwijderen mislukt: {error}");
            }
        }
    }
}