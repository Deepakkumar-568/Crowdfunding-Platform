using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Antigravity.Application.Common.Interfaces;
using Antigravity.Application.DTOs;
using Antigravity.Domain.Entities;
using Antigravity.Domain.Enums;

namespace Antigravity.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == request.Email.ToLower());
        var user = users.FirstOrDefault();

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtService.GenerateToken(user);
        var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role);

        return new AuthResponse(token, userDto);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Check uniqueness
        var existingEmail = (await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == request.Email.ToLower())).FirstOrDefault();
        if (existingEmail != null) return null;

        var existingUser = (await _unitOfWork.Users.FindAsync(u => u.Username.ToLower() == request.Username.ToLower())).FirstOrDefault();
        if (existingUser != null) return null;

        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(newUser);

        // Add a welcome notification
        var welcomeNotif = new Notification
        {
            UserId = newUser.Id,
            Message = $"Welcome to Antigravity, {newUser.Username}! Your account as an {newUser.Role} is successfully created.",
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Notifications.AddAsync(welcomeNotif);

        await _unitOfWork.SaveChangesAsync();

        var token = _jwtService.GenerateToken(newUser);
        var userDto = new UserDto(newUser.Id, newUser.Username, newUser.Email, newUser.Role);

        return new AuthResponse(token, userDto);
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == request.Email.ToLower());
        var user = users.FirstOrDefault();
        if (user == null) return false;

        // In production, send email with verification token
        // Here we mock sending email and create a dashboard notification for visual proof
        var notif = new Notification
        {
            UserId = user.Id,
            Message = "A password reset request was initiated. Use security token 'RESET123' to reset your password.",
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Notifications.AddAsync(notif);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == request.Email.ToLower());
        var user = users.FirstOrDefault();
        if (user == null) return false;

        // Verify simulated token
        if (request.Token != "RESET123" && request.Token != "DEMO_TOKEN") return false;

        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        var notif = new Notification
        {
            UserId = user.Id,
            Message = "Your password has been successfully reset. If you did not make this change, contact support.",
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Notifications.AddAsync(notif);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}
