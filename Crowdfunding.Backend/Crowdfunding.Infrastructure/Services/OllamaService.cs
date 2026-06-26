using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Crowdfunding.Application.Common.Interfaces;
using Crowdfunding.Application.DTOs;

namespace Crowdfunding.Infrastructure.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly string _baseUrl;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _model = configuration["Ollama:Model"] ?? "llama3";
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(120);
    }

    public async Task<string> EnhanceDescriptionAsync(string name, string tagline, string description)
    {
        var prompt = $"You are a professional copywriter for startup fundraising.\n" +
                     $"Startup Name: {name}\n" +
                     $"Tagline: {tagline}\n" +
                     $"Original Description: {description}\n" +
                     $"Task: Enhance this description to make it highly premium, professional, and exciting for institutional and retail investors. Keep it under 200 words, using clean, compelling language.";

        return await GenerateTextAsync(prompt);
    }

    public async Task<AIPitchRefineResponse> ImprovePitchAsync(string name, string pitch)
    {
        var prompt = 
            "You are an experienced Silicon Valley Venture Capital advisor and startup mentor.\n" +
            "Your task is to convert startup ideas into professional investor-ready pitch decks.\n" +
            "Always produce well-formatted Markdown for the pitch deck.\n" +
            "If the user's idea lacks information, intelligently complete the missing details using realistic startup assumptions.\n" +
            "Use professional investor language.\n\n" +
            "You must return the response as a JSON object conforming exactly to this structure:\n" +
            "{\n" +
            "  \"success\": true,\n" +
            "  \"generatedPitch\": \"The complete refined pitch deck in Markdown format. Make sure to generate the following sections in the Markdown: # Executive Summary, # Problem, # Solution, # Market Opportunity, # Target Customers, # Business Model, # Revenue Model, # Competitive Advantage, # Go-To-Market Strategy, # Marketing Strategy, # Product Roadmap, # Financial Projections, # Funding Requirement, # Use of Funds, # Risks, # Why Investors Should Invest, # Call to Action.\",\n" +
            "  \"score\": 92,\n" +
            "  \"fundingStage\": \"Seed\",\n" +
            "  \"estimatedValuation\": \"$2M - $5M\",\n" +
            "  \"suggestedFunding\": \"$500K\",\n" +
            "  \"strengths\": [\"Strength 1\", \"Strength 2\"],\n" +
            "  \"weaknesses\": [\"Weakness 1\", \"Weakness 2\"],\n" +
            "  \"risks\": [\"Risk 1\", \"Risk 2\"],\n" +
            "  \"investorQuestions\": [\"Question 1\", \"Question 2\"]\n" +
            "}\n\n" +
            "Guidelines (CRITICAL FOR PERFORMANCE):\n" +
            "- Make sure to generate all 17 markdown headers listed above in the 'generatedPitch' field.\n" +
            "- You MUST keep each section in 'generatedPitch' extremely short (exactly 1 to 2 short sentences per header).\n" +
            "- The total length of the 'generatedPitch' must be under 350 words. Do not write long paragraphs.\n" +
            "- The strengths, weaknesses, risks, and investorQuestions arrays must contain exactly 3 concise, startup-specific bullet points matching the startup idea.\n" +
            "- Return valid JSON. Do not add any text before or after the JSON block.\n\n" +
            $"Startup Idea from Founder of '{name}': {pitch}";

        try
        {
            var rawResult = await GenerateTextAsync(prompt, formatJson: true);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<AIPitchRefineResponse>(rawResult, options);
            if (response != null)
            {
                return response with { Success = true };
            }
            throw new Exception("Failed to deserialize JSON response from Ollama.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refining pitch via Ollama for startup '{Name}'", name);
            return new AIPitchRefineResponse(
                Success: false,
                GeneratedPitch: "AI Pitch Refiner is currently unavailable. Please check your local Ollama connection (make sure the Ollama server is running at http://localhost:11434 and the model is fully pulled/loaded). Details: " + ex.Message,
                Score: 0,
                FundingStage: "N/A",
                EstimatedValuation: "N/A",
                SuggestedFunding: "N/A",
                Strengths: Array.Empty<string>(),
                Weaknesses: Array.Empty<string>(),
                Risks: new string[] { "Local Ollama service offline or model error" },
                InvestorQuestions: Array.Empty<string>()
            );
        }
    }

    public async Task<string> GenerateSummaryAsync(string name, string description, string businessModel, string financials)
    {
        var prompt = $"Synthesize a concise, 3-bullet-point executive summary for '{name}' based on these details:\n" +
                     $"- Description: {description}\n" +
                     $"- Business Model: {businessModel}\n" +
                     $"- Financial Overview: {financials}\n" +
                     $"Format the response as bullet points (starting with '-') outlining: Value Prop, Revenue Model, and Financial/Growth Targets.";

        return await GenerateTextAsync(prompt);
    }

    public async Task<string> GetInvestorRecommendationsAsync(string investorInterests, string startupListJson)
    {
        var prompt = $"You are an investment analyst.\n" +
                     $"Investor Interests: {investorInterests}\n" +
                     $"Startup List (JSON format): {startupListJson}\n" +
                     $"Task: Return a friendly recommendations analysis in markdown format, recommending the top 2 startups that align best with the investor's interests. Be direct and concise.";

        return await GenerateTextAsync(prompt);
    }

    public async Task<int> CalculateHealthScoreAsync(string name, decimal fundingGoal, decimal currentFunding, int teamCount)
    {
        var prompt = $"Calculate a startup health score from 0 to 100 based on:\n" +
                     $"- Name: {name}\n" +
                     $"- Funding Goal: ${fundingGoal}\n" +
                     $"- Current Funding: ${currentFunding}\n" +
                     $"- Team Size: {teamCount} members\n" +
                     $"Task: Respond ONLY with a single integer between 0 and 100 representing the score. Do not write anything else.";

        try
        {
            var rawResult = await GenerateTextAsync(prompt);
            var digits = new StringBuilder();
            foreach (var c in rawResult)
            {
                if (char.IsDigit(c)) digits.Append(c);
            }

            if (int.TryParse(digits.ToString(), out var score))
            {
                return Math.Clamp(score, 10, 100);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to calculate health score using Ollama for '{Name}'. Falling back to math logic.", name);
        }

        // Standard logic calculation fallback
        var ratio = fundingGoal > 0 ? (double)(currentFunding / fundingGoal) : 0;
        var calculated = 60 + (int)(ratio * 30) + Math.Min(teamCount * 2, 10);
        return Math.Clamp(calculated, 10, 100);
    }

    private async Task<string> GenerateTextAsync(string prompt, bool formatJson = false)
    {
        try
        {
            var requestBody = new OllamaRequest
            {
                Model = _model,
                Prompt = prompt,
                Stream = false,
                Format = formatJson ? "json" : null,
                Options = formatJson ? new OllamaOptions { Temperature = 0.2f, NumPredict = 800 } : null
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/generate", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseBody);
                if (ollamaResponse != null && !string.IsNullOrWhiteSpace(ollamaResponse.Response))
                {
                    return ollamaResponse.Response.Trim();
                }
            }

            throw new Exception($"Ollama API returned non-success code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call local Ollama API");
            throw new Exception("Ollama service is offline or returned an error. Please verify Ollama is running locally on port 11434.", ex);
        }
    }

    private class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = null!;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = null!;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Format { get; set; }

        [JsonPropertyName("options")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OllamaOptions? Options { get; set; }
    }

    private class OllamaOptions
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; }
    }

    private class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = null!;
    }
}
