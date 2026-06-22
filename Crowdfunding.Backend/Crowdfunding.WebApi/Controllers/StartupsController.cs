using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crowdfunding.Application.Common.Interfaces;
using Crowdfunding.Application.DTOs;
using Crowdfunding.Domain.Enums;

namespace Crowdfunding.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StartupsController : ControllerBase
{
    private readonly IStartupService _startupService;

    public StartupsController(IStartupService startupService)
    {
        _startupService = startupService;
    }

    [HttpPost]
    [Authorize(Roles = "Founder,Admin")]
    public async Task<IActionResult> Create([FromBody] StartupCreateRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var response = await _startupService.CreateStartupAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? industry,
        [FromQuery] string? sortBy)
    {
        var response = await _startupService.GetAllStartupsAsync(search, category, industry, sortBy);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var response = await _startupService.GetStartupByIdAsync(id);
        if (response == null) return NotFound(new { Message = "Startup profile not found." });
        return Ok(response);
    }

    [HttpPut("{id}/verify")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Verify(string id, [FromBody] StartupVerificationRequest request)
    {
        var result = await _startupService.VerifyStartupAsync(id, request.Status);
        if (!result) return NotFound(new { Message = "Startup profile not found." });
        return Ok(new { Message = $"Startup profile has been updated to '{request.Status}'." });
    }

    [HttpPost("{id}/save")]
    [Authorize(Roles = "Investor")]
    public async Task<IActionResult> ToggleSave(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var isSaved = await _startupService.ToggleSaveStartupAsync(id, userId);
        return Ok(new { IsSaved = isSaved, Message = isSaved ? "Startup saved to watchlist." : "Startup removed from watchlist." });
    }

    [HttpGet("saved")]
    [Authorize(Roles = "Investor")]
    public async Task<IActionResult> GetSaved()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var response = await _startupService.GetSavedStartupsAsync(userId);
        return Ok(response);
    }
}
