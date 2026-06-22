using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crowdfunding.Application.Common.Interfaces;
using Crowdfunding.Application.DTOs;
using Crowdfunding.Domain.Enums;

namespace Crowdfunding.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public CampaignsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpPost]
    [Authorize(Roles = "Founder,Admin")]
    public async Task<IActionResult> Create([FromBody] CampaignCreateRequest request)
    {
        try
        {
            var response = await _campaignService.CreateCampaignAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] decimal? minGoal,
        [FromQuery] decimal? maxGoal,
        [FromQuery] string? sortBy)
    {
        var response = await _campaignService.GetAllCampaignsAsync(search, minGoal, maxGoal, sortBy);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var response = await _campaignService.GetCampaignByIdAsync(id);
        if (response == null) return NotFound(new { Message = "Campaign not found." });
        return Ok(response);
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(string id, [FromBody] CampaignApprovalRequest request)
    {
        var result = await _campaignService.ApproveCampaignAsync(id, request.Status);
        if (!result) return NotFound(new { Message = "Campaign not found." });
        return Ok(new { Message = $"Campaign status has been updated to '{request.Status}'." });
    }

    [HttpGet("startup/{startupId}")]
    public async Task<IActionResult> GetByStartup(string startupId)
    {
        try
        {
            var response = await _campaignService.GetCampaignsByStartupIdAsync(startupId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
