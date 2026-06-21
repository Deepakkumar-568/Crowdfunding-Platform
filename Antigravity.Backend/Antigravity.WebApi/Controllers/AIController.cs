using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.Application.Common.Interfaces;
using Antigravity.Application.DTOs;

namespace Antigravity.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IOllamaService _ollamaService;

    public AIController(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    [HttpPost("enhance-description")]
    [Authorize(Roles = "Founder,Admin")]
    public async Task<IActionResult> EnhanceDescription([FromBody] AIEnhanceDescriptionRequest request)
    {
        var result = await _ollamaService.EnhanceDescriptionAsync(request.Name, request.Tagline, request.Description);
        return Ok(new AITextResponse(result));
    }

    [HttpPost("improve-pitch")]
    [Authorize(Roles = "Founder,Admin")]
    public async Task<IActionResult> ImprovePitch([FromBody] AIImprovePitchRequest request)
    {
        var result = await _ollamaService.ImprovePitchAsync(request.Name, request.Pitch);
        return Ok(new AITextResponse(result));
    }

    [HttpPost("generate-summary")]
    public async Task<IActionResult> GenerateSummary([FromBody] AIGenerateSummaryRequest request)
    {
        var result = await _ollamaService.GenerateSummaryAsync(request.Name, request.Description, request.BusinessModel, request.Financials);
        return Ok(new AITextResponse(result));
    }

    [HttpPost("recommendations")]
    [Authorize(Roles = "Investor")]
    public async Task<IActionResult> GetRecommendations([FromBody] AIRecommendationRequest request, [FromServices] IStartupService startupService)
    {
        // Get all startups to feed to the recommendation engine
        var startups = await startupService.GetAllStartupsAsync(null, null, null, null);
        var startupListJson = System.Text.Json.JsonSerializer.Serialize(startups);

        var result = await _ollamaService.GetInvestorRecommendationsAsync(request.InvestorInterests, startupListJson);
        return Ok(new AITextResponse(result));
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
