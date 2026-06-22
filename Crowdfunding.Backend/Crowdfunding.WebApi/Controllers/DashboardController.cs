using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crowdfunding.Application.Common.Interfaces;

namespace Crowdfunding.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("founder")]
    [Authorize(Roles = "Founder")]
    public async Task<IActionResult> GetFounderDashboard()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var response = await _dashboardService.GetFounderDashboardAsync(userId);
        return Ok(response);
    }

    [HttpGet("investor")]
    [Authorize(Roles = "Investor")]
    public async Task<IActionResult> GetInvestorDashboard()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var response = await _dashboardService.GetInvestorDashboardAsync(userId);
        return Ok(response);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var response = await _dashboardService.GetAdminDashboardAsync();
        return Ok(response);
    }
}
