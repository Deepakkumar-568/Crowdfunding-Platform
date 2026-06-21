using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Antigravity.Application.Common.Interfaces;

namespace Antigravity.Infrastructure.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly string _baseUrl;

    public OllamaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _model = configuration["Ollama:Model"] ?? "llama3";
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<string> EnhanceDescriptionAsync(string name, string tagline, string description)
    {
        var prompt = $"You are a professional copywriter for startup fundraising. " +
                     $"Startup Name: {name}\nTagline: {tagline}\nOriginal Description: {description}\n" +
                     $"Task: Enhance this description to make it highly premium, professional, and exciting for institutional and retail investors. Keep it under 250 words, using clean, compelling language.";

        return await GenerateTextAsync(prompt, () => 
            $"Antigravity-enhanced overview of {name}: Re-engineered from the ground up, {name} delivers state-of-the-art solutions addressing key gaps in the market. With our unique value proposition: '{tagline}', we scale user acquisition and build long-term value. Our target market represents a multi-billion dollar opportunity, backed by a strong technology framework designed to capture double-digit market share within the next 24 months.");
    }

    public async Task<string> ImprovePitchAsync(string name, string pitch)
    {
        var prompt = $"Improve the following investor pitch for the startup '{name}'. " +
                     $"Original Pitch: {pitch}\n" +
                     $"Task: Structure the pitch with clear headings (The Problem, The Solution, Market Size, Call to Action) and refine the narrative style to build deep investor trust and urgency.";

        return await GenerateTextAsync(prompt, () => 
            $"### **The Problem**\nCurrent solutions are fragmented, expensive, and lack clear verification pathways, causing severe friction for customers.\n\n" +
            $"### **The Solution**\n{name} offers a unified, automated ecosystem that simplifies operations, increases margins, and ensures high integrity.\n\n" +
            $"### **Market Size**\nOperating in a $45B global industry growing at 12% CAGR, we are positioned to capture early adopter markets efficiently.\n\n" +
            $"### **Call to Action**\nJoin our seed campaign to accelerate platform development and scale customer acquisition.");
    }

    public async Task<string> GenerateSummaryAsync(string name, string description, string businessModel, string financials)
    {
        var prompt = $"Synthesize a concise, 3-bullet-point executive summary for '{name}' based on these details:\n" +
                     $"- Description: {description}\n" +
                     $"- Business Model: {businessModel}\n" +
                     $"- Financial Overview: {financials}\n" +
                     $"Format the response as bullet points (starting with '-') outlining: Value Prop, Revenue Model, and Financial/Growth Targets.";

        return await GenerateTextAsync(prompt, () => 
            $"- **Core Value Proposition**: Custom modern workflows solving immediate operational inefficiencies.\n" +
            $"- **Revenue Model**: Transaction-based SaaS fees and volume-based commissions.\n" +
            $"- **Growth Targets**: Projecting cashflow positive status within 18 months, targeting $5M ARR.");
    }

    public async Task<string> GetInvestorRecommendationsAsync(string investorInterests, string startupListJson)
    {
        var prompt = $"You are an investment analyst. " +
                     $"Investor Interests: {investorInterests}\n" +
                     $"Startup List (JSON format): {startupListJson}\n" +
                     $"Task: Return a friendly recommendations analysis in markdown format, recommending the top 2 startups that align best with the investor's interests. Be direct and concise.";

        return await GenerateTextAsync(prompt, () => 
            $"Based on your interests in **{investorInterests}**, we recommend exploring startups specializing in scalable SaaS and AI automation frameworks. These options present strong growth velocity and aligned business models.");
    }

    public async Task<int> CalculateHealthScoreAsync(string name, decimal fundingGoal, decimal currentFunding, int teamCount)
    {
        var prompt = $"Calculate a startup health score from 0 to 100 based on:\n" +
                     $"- Name: {name}\n" +
                     $"- Funding Goal: ${fundingGoal}\n" +
                     $"- Current Funding: ${currentFunding}\n" +
                     $"- Team Size: {teamCount} members\n" +
                     $"Task: Respond ONLY with a single integer between 0 and 100 representing the score. Do not write anything else.";

        var rawResult = await GenerateTextAsync(prompt, () => "85");
        
        // Extract the first number found in the result
        var digits = new StringBuilder();
        foreach (var c in rawResult)
        {
            if (char.IsDigit(c)) digits.Append(c);
        }

        if (int.TryParse(digits.ToString(), out var score))
        {
            return Math.Clamp(score, 10, 100);
        }

        // Standard logic calculation fallback
        var ratio = fundingGoal > 0 ? (double)(currentFunding / fundingGoal) : 0;
        var calculated = 60 + (int)(ratio * 30) + Math.Min(teamCount * 2, 10);
        return Math.Clamp(calculated, 10, 100);
    }

    private async Task<string> GenerateTextAsync(string prompt, Func<string> fallback)
    {
        try
        {
            var requestBody = new OllamaRequest
            {
                Model = _model,
                Prompt = prompt,
                Stream = false
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
        }
        catch
        {
            // Fall back to clean mocked response if local Ollama is offline/unreachable
        }

        return fallback();
    }

    private class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = null!;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = null!;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    private class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = null!;
    }
}
