using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Crowdfunding.Application.Common.Interfaces;
using Crowdfunding.Application.DTOs;

namespace Crowdfunding.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        if (response == null)
        {
            return BadRequest(new { Message = "Registration failed. Username or email may already be taken." });
        }
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { Message = "Invalid email or password." });
        }
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        if (!result)
        {
            return NotFound(new { Message = "Email address not found." });
        }
        return Ok(new { Message = "Password reset instructions have been sent to your email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        if (!result)
        {
            return BadRequest(new { Message = "Invalid email token or token has expired." });
        }
        return Ok(new { Message = "Your password has been successfully reset." });
    }
}
