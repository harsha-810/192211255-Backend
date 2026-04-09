using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace PetClinicAPI.Services;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiService> _logger;
    private readonly string _apiKey;
    private readonly string _hfApiKey;

    public AiService(HttpClient httpClient, IConfiguration configuration, ILogger<AiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["Gemini:ApiKey"] ?? "";
        _hfApiKey = _configuration["HuggingFace:ApiKey"] ?? "";
    }

    public async Task<(string condition, string severity, string recommendation, int priorityLevel)> AnalyzeSymptomsAsync(List<string> symptoms, string duration, int petAge, string species, string breed)
    {
        if (symptoms == null || !symptoms.Any())
        {
            return ("Unknown Status", "Low", "No symptoms provided. Please monitor your pet and ensure they have plenty of water and rest.", 3);
        }

        // 1. Check for Critical Emergency Symptoms immediately (Safety Layer)
        var emergencyCheck = CheckForEmergency(symptoms);
        if (emergencyCheck != null) return emergencyCheck.Value;

        // 2. Attempt Real AI Analysis via Gemini (Primary)
        if (!string.IsNullOrEmpty(_apiKey) && _apiKey != "YOUR_GEMINI_API_KEY")
        {
            try
            {
                var result = await GetGeminiAnalysisAsync(symptoms, duration, petAge, species, breed);
                if (result != null) return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[AI-FALLBACK] Gemini failed: {ex.Message}. Trying Local Model...");
            }
        }



        // 4. Attempt Backup AI Analysis via Hugging Face API
        if (!string.IsNullOrEmpty(_hfApiKey))
        {
            try
            {
                var result = await GetHuggingFaceAnalysisAsync(symptoms, duration, petAge, species, breed);
                if (result != null) return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[AI-FALLBACK] Hugging Face API failed: {ex.Message}. Falling back to mock engine.");
            }
        }

        // 5. Final Fallback to Mock Engine
        return GetMockAnalysis(symptoms, duration);
    }

    private (string, string, string, int)? CheckForEmergency(List<string> symptoms)
    {
        var lowerSymptoms = symptoms.Select(s => s.ToLower()).ToList();
        var emergencyKeywords = new[] { "breathing difficulty", "unconscious", "seizures", "bleeding", "unresponsive" };
        
        bool hasEmergency = lowerSymptoms.Any(s => emergencyKeywords.Any(k => s.Contains(k)));

        if (hasEmergency)
        {
            string detail = "The symptoms described indicate a critical compromise of vital functions. ";
            if (lowerSymptoms.Any(s => s.Contains("breathing difficulty"))) detail += "Respiratory distress can rapidly lead to hypoxia. ";
            if (lowerSymptoms.Any(s => s.Contains("seizures"))) detail += "Neurological episodes may indicate toxic ingestion or severe metabolic imbalance. ";
            if (lowerSymptoms.Any(s => s.Contains("bleeding"))) detail += "Active hemorrhage requires immediate pressure and professional intervention. ";

            return (
                "Critical Emergency Priority",
                "High",
                $"{detail}\n\nIMMEDIATE ACTIONS:\n1. Transport to the nearest 24/7 emergency clinic immediately.\n2. Keep the pet warm and minimize stimulation.\n3. Do not attempt to give food or water orally.",
                1
            );
        }
        return null;
    }



    private async Task<(string condition, string severity, string recommendation, int priorityLevel)?> GetHuggingFaceAnalysisAsync(List<string> symptoms, string duration, int petAge, string species, string breed)
    {
        var modelId = "meta-llama/Meta-Llama-3.1-8B-Instruct";
        var url = $"https://api-inference.huggingface.co/models/{modelId}";

        var prompt = $@"
        Analyze these pet symptoms for a {petAge} year old {breed} {species}.
        Symptoms: {string.Join(", ", symptoms)}
        Duration: {duration}

        Return ONLY a JSON object:
        {{
            ""condition"": ""Possible condition name"",
            ""severity"": ""High/Medium/Low"",
            ""recommendation"": ""Short actionable advice"",
            ""priorityLevel"": 1, 2, or 3
        }}";

        var requestBody = new { inputs = prompt };
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _hfApiKey);

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var text = doc.RootElement[0].GetProperty("generated_text").GetString();

            if (!string.IsNullOrEmpty(text))
            {
                var match = Regex.Match(text, @"\{.*\}", RegexOptions.Singleline);
                if (match.Success)
                {
                    var aiResult = JsonSerializer.Deserialize<AiResultInternal>(match.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (aiResult != null)
                    {
                        return (aiResult.Condition, aiResult.Severity, aiResult.Recommendation, aiResult.PriorityLevel);
                    }
                }
            }
        }
        return null;
    }

    private async Task<(string condition, string severity, string recommendation, int priorityLevel)?> GetGeminiAnalysisAsync(List<string> symptoms, string duration, int petAge, string species, string breed)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        var prompt = $@"
        You are a professional veterinary assistant. Analyze symptoms and provide responses ONLY in JSON format.
        
        PET PROFILE:
        - Species: {species}
        - Breed: {breed}
        - Age: {petAge} years old

        CONSULTATION DETAILS:
        - Symptoms: {string.Join(", ", symptoms)}
        - Duration: {duration}

        Return response ONLY as valid JSON:
        {{
            ""condition"": ""Possible condition name"",
            ""severity"": ""High/Medium/Low"",
            ""recommendation"": ""Specific actionable advice"",
            ""priorityLevel"": 1, 2, or 3
        }}";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

            if (!string.IsNullOrEmpty(text))
            {
                var match = Regex.Match(text, @"\{.*\}", RegexOptions.Singleline);
                if (match.Success)
                {
                    var aiResult = JsonSerializer.Deserialize<AiResultInternal>(match.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (aiResult != null)
                    {
                        return (aiResult.Condition, aiResult.Severity, aiResult.Recommendation, aiResult.PriorityLevel);
                    }
                }
            }
        }
        return null;
    }

    private (string condition, string severity, string recommendation, int priorityLevel) GetMockAnalysis(List<string> symptoms, string duration)
    {
        var lowerSymptoms = symptoms.Select(s => s.ToLower()).ToList();

        if (lowerSymptoms.Contains("fever") && (lowerSymptoms.Contains("vomiting") || lowerSymptoms.Contains("diarrhea")))
        {
            return ("Severe Systemic Infection", "High", "Active inflammatory/infectious process detected. Immediate physical exam required.", 1);
        }

        if (lowerSymptoms.Contains("vomiting") || lowerSymptoms.Contains("diarrhea"))
        {
            return ("Acute Gastroenteritis", "Medium", "Often caused by dietary indiscretion. Fast pet for 12h, then bland diet.", 2);
        }

        return ("Wellness Analysis Recommended", "Low", "Symptoms are broad. Monitor for 48h; if no improvement, schedule a wellness exam.", 3);
    }

    private class AiResultInternal
    {
        [JsonPropertyName("condition")]
        public string Condition { get; set; } = "";

        [JsonPropertyName("severity")]
        public string Severity { get; set; } = "";

        [JsonPropertyName("recommendation")]
        public string Recommendation { get; set; } = "";

        [JsonPropertyName("priorityLevel")]
        public int PriorityLevel { get; set; } = 3;
    }
}
