using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crowdfunding.Application.Common.Interfaces;
using Crowdfunding.Application.DTOs;

namespace Crowdfunding.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvestmentsController : ControllerBase
{
    private readonly IInvestmentService _investmentService;

    public InvestmentsController(IInvestmentService investmentService)
    {
        _investmentService = investmentService;
    }

    [HttpPost]
    [Authorize(Roles = "Investor")]
    public async Task<IActionResult> Create([FromBody] InvestmentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var response = await _investmentService.CreateInvestmentAsync(request, userId);
            if (response == null) return BadRequest(new { Message = "Investment failed. Campaign may not exist." });
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Investor")]
    public async Task<IActionResult> GetMyInvestments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var response = await _investmentService.GetInvestmentsByInvestorIdAsync(userId);
        return Ok(response);
    }

    [HttpGet("campaign/{campaignId}")]
    [Authorize(Roles = "Founder,Admin")]
    public async Task<IActionResult> GetByCampaign(string campaignId)
    {
        var response = await _investmentService.GetInvestmentsByCampaignIdAsync(campaignId);
        return Ok(response);
    }
}
