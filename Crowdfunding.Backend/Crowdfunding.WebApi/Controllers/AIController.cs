using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Crowdfunding.Application.Common.Interfaces;
using Crowdfunding.Application.DTOs;

namespace Crowdfunding.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<AIController> _logger;

    public AIController(IOllamaService ollamaService, ILogger<AIController> logger)
    {
        _ollamaService = ollamaService;
        _logger = logger;
    }

    [HttpPost("enhance-description")]
    [Authorize(Roles = "Founder,Admin")]
    public async Task<IActionResult> EnhanceDescription([FromBody] AIEnhanceDescriptionRequest request)
    {
        try
        {
            var result = await _ollamaService.EnhanceDescriptionAsync(request.Name, request.Tagline, request.Description);
            return Ok(new AITextResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing description for '{Name}'", request.Name);
            return StatusCode(503, new { message = "AI Copywriting service is currently offline. Please check your local Ollama connection." });
        }
    }

    [HttpPost("improve-pitch")]
    [Authorize(Roles = "Founder,Admin")]
    public async Task<IActionResult> ImprovePitch([FromBody] AIImprovePitchRequest request)
    {
        var result = await _ollamaService.ImprovePitchAsync(request.Name, request.Pitch);
        return Ok(result);
    }

    [HttpPost("generate-summary")]
    public async Task<IActionResult> GenerateSummary([FromBody] AIGenerateSummaryRequest request)
    {
        try
        {
            var result = await _ollamaService.GenerateSummaryAsync(request.Name, request.Description, request.BusinessModel, request.Financials);
            return Ok(new AITextResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary for '{Name}'", request.Name);
            return StatusCode(503, new { message = "AI Summary service is currently offline. Please check your local Ollama connection." });
        }
    }

    [HttpPost("recommendations")]
    [Authorize(Roles = "Investor")]
    public async Task<IActionResult> GetRecommendations([FromBody] AIRecommendationRequest request, [FromServices] IStartupService startupService)
    {
        try
        {
            // Get all startups to feed to the recommendation engine
            var startups = await startupService.GetAllStartupsAsync(null, null, null, null);
            var startupListJson = System.Text.Json.JsonSerializer.Serialize(startups);

            var result = await _ollamaService.GetInvestorRecommendationsAsync(request.InvestorInterests, startupListJson);
            return Ok(new AITextResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting investor recommendations");
            return StatusCode(503, new { message = "AI Recommendation service is currently offline. Please check your local Ollama connection." });
        }
    }

    [HttpGet("health-score")]
    public async Task<IActionResult> GetHealthScore(
        [FromQuery] string name,
        [FromQuery] decimal goal,
        [FromQuery] decimal funding,
        [FromQuery] int teamCount)
    {
        var score = await _ollamaService.CalculateHealthScoreAsync(name, goal, funding, teamCount);
        return Ok(new { HealthScore = score });
    }
}
