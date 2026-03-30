using System.Text;
using System.Text.Json;
using AutoReportGenerator.DTOs;

namespace AutoReportGenerator.Services;

/// <summary>
/// AI-powered report enhancement with multiple free provider fallback
/// </summary>
public class AiReportEnhancementService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiReportEnhancementService> _logger;

    // Provider priority order
    private readonly List<AiProvider> _providers = new()
    {
        new AiProvider { Name = "Groq", Endpoint = "https://api.groq.com/openai/v1/chat/completions", Model = "llama-3.1-8b-instant" },
        new AiProvider { Name = "HuggingFace", Endpoint = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.2", Model = "" },
        new AiProvider { Name = "TogetherAI", Endpoint = "https://api.together.xyz/v1/chat/completions", Model = "mistralai/Mixtral-8x7B-Instruct-v0.1" }
    };

    public AiReportEnhancementService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<AiReportEnhancementService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Enhance report with AI insights using fallback chain
    /// Returns (insights, providerName) tuple
    /// </summary>
    public async Task<(string insights, string provider)> EnhanceReportAsync(string rawText, string reportType, SummaryMetrics metrics)
    {
        var prompt = BuildEnhancementPrompt(rawText, reportType, metrics);

        // Try each provider in order
        foreach (var provider in _providers)
        {
            try
            {
                var apiKey = _configuration[$"AI:{provider.Name}:ApiKey"];
                
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogInformation($"No API key configured for {provider.Name}, skipping...");
                    continue;
                }

                _logger.LogInformation($"Attempting AI enhancement with {provider.Name}...");
                
                var result = await CallProviderAsync(provider, apiKey, prompt);
                
                if (!string.IsNullOrWhiteSpace(result))
                {
                    _logger.LogInformation($"Successfully enhanced report using {provider.Name}");
                    return (result, provider.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"{provider.Name} failed: {ex.Message}. Trying next provider...");
                continue;
            }
        }

        // All providers failed, return empty (will use heuristic fallback)
        _logger.LogInformation("All AI providers failed or unavailable, using heuristic analysis");
        return (string.Empty, "Heuristic");
    }

    private async Task<string?> CallProviderAsync(AiProvider provider, string apiKey, string prompt)
    {
        return provider.Name switch
        {
            "Groq" => await CallGroqAsync(provider, apiKey, prompt),
            "HuggingFace" => await CallHuggingFaceAsync(provider, apiKey, prompt),
            "TogetherAI" => await CallTogetherAIAsync(provider, apiKey, prompt),
            _ => null
        };
    }

    private async Task<string?> CallGroqAsync(AiProvider provider, string apiKey, string prompt)
    {
        var request = new
        {
            model = provider.Model,
            messages = new[]
            {
                new { role = "system", content = "You are a professional report analyst. Provide concise, actionable insights." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 500
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, provider.Endpoint);
        httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Groq API error: {response.StatusCode} - {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);
        
        return result.GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }

    private async Task<string?> CallHuggingFaceAsync(AiProvider provider, string apiKey, string prompt)
    {
        var request = new
        {
            inputs = prompt,
            parameters = new
            {
                max_new_tokens = 500,
                temperature = 0.7,
                return_full_text = false
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, provider.Endpoint);
        httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"HuggingFace API error: {response.StatusCode} - {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);
        
        return result[0].GetProperty("generated_text").GetString();
    }

    private async Task<string?> CallTogetherAIAsync(AiProvider provider, string apiKey, string prompt)
    {
        var request = new
        {
            model = provider.Model,
            messages = new[]
            {
                new { role = "system", content = "You are a professional report analyst. Provide concise, actionable insights." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 500
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, provider.Endpoint);
        httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"TogetherAI API error: {response.StatusCode} - {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);
        
        return result.GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }

    private string BuildEnhancementPrompt(string rawText, string reportType, SummaryMetrics metrics)
    {
        return $@"Analyze this {reportType} and provide key insights, opportunities, risks, and recommendations.

Report Content:
{rawText.Substring(0, Math.Min(rawText.Length, 2000))}

Metrics:
- Total Activities: {metrics.TotalActivities}
- Completion Rate: {metrics.CompletionRate}%
- Completed: {metrics.CompletedCount}
- Pending: {metrics.PendingCount}

Instructions:
- Use markdown formatting in your response.
- When the content contains tabular or comparative data (e.g. tasks by status, categories, financial figures, scores), present it as a markdown table.
- Example table format:
  | Category | Count | Status |
  |----------|-------|--------|
  | Development | 3 | Completed |
- Use bullet points for non-tabular insights.

Provide output in this format:

## Key Insights
• [insight]

## Summary Table
[Include a markdown table here if the data warrants it, otherwise omit this section]

## Opportunities
• [opportunity]

## Risks
• [risk]

## Recommendations
• [recommendation]

Keep it concise and actionable.";
    }

    private class AiProvider
    {
        public string Name { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}
