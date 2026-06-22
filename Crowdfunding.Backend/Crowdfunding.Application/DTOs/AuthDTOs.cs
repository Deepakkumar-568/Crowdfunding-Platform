using Crowdfunding.Domain.Enums;

namespace Crowdfunding.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Username, string Email, string Password, Role Role);

public record UserDto(string Id, string Username, string Email, Role Role);

public record AuthResponse(string Token, UserDto User);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);
