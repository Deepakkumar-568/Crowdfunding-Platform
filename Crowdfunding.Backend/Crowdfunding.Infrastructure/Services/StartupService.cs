using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Crowdfunding.Application.Common.Interfaces;
using Crowdfunding.Application.DTOs;
using Crowdfunding.Domain.Entities;
using Crowdfunding.Domain.Enums;
using Crowdfunding.Infrastructure.Persistence;

namespace Crowdfunding.Infrastructure.Services;

public class StartupService : IStartupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly IOllamaService _ollamaService;

    public StartupService(IUnitOfWork unitOfWork, ApplicationDbContext dbContext, IOllamaService ollamaService)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _ollamaService = ollamaService;
    }

    public async Task<StartupResponse> CreateStartupAsync(StartupCreateRequest request, string founderId)
    {
        var founder = await _unitOfWork.Users.GetByIdAsync(founderId) 
            ?? throw new ArgumentException("Founder user not found.");

        var startup = new Startup
        {
            FounderId = founderId,
            Name = request.Name,
            Tagline = request.Tagline,
            Industry = request.Industry,
            Category = request.Category,
            Description = request.Description,
            BusinessModel = request.BusinessModel,
            FinancialOverview = request.FinancialOverview,
            LogoUrl = request.LogoUrl,
            PitchDeckUrl = request.PitchDeckUrl,
            WebsiteUrl = request.WebsiteUrl,
            VideoUrl = request.VideoUrl,
            Status = StartupStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Add team members
        if (request.TeamMembers != null)
        {
            foreach (var tm in request.TeamMembers)
            {
                startup.TeamMembers.Add(new TeamMember
                {
                    Name = tm.Name,
                    Role = tm.Role,
                    Bio = tm.Bio,
                    AvatarUrl = tm.AvatarUrl
                });
            }
        }

        // AI generated Health Score calculation
        startup.HealthScore = await _ollamaService.CalculateHealthScoreAsync(
            startup.Name, 
            100000m, 
            0m, 
            request.TeamMembers?.Count ?? 0
        );

        await _unitOfWork.Startups.AddAsync(startup);

        // Notify Admins
        var admins = await _unitOfWork.Users.FindAsync(u => u.Role == Role.Admin);
        foreach (var admin in admins)
        {
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = admin.Id,
                Message = $"New startup profile '{startup.Name}' submitted by {founder.Username} is pending verification.",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(startup, founder.Username);
    }

    public async Task<StartupResponse?> GetStartupByIdAsync(string id)
    {
        // Load with includes using EF DbContext directly for complex relationships in clean way
        var startup = await _dbContext.Startups
            .Include(s => s.Founder)
            .Include(s => s.TeamMembers)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (startup == null) return null;

        return MapToResponse(startup, startup.Founder.Username);
    }

    public async Task<IEnumerable<StartupResponse>> GetAllStartupsAsync(string? search, string? category, string? industry, string? sortBy)
    {
        var query = _dbContext.Startups
            .Include(s => s.Founder)
            .Include(s => s.TeamMembers)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(searchLower) || 
                                     s.Tagline.ToLower().Contains(searchLower) ||
                                     s.Description.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(s => s.Category.ToLower() == category.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(industry))
        {
            query = query.Where(s => s.Industry.ToLower() == industry.ToLower());
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "popularity" => query.OrderByDescending(s => s.SavedByInvestors.Count),
            "health" => query.OrderByDescending(s => s.HealthScore),
            "newest" => query.OrderByDescending(s => s.CreatedAt),
            _ => query.OrderBy(s => s.Name)
        };

        var startups = await query.ToListAsync();
        return startups.Select(s => MapToResponse(s, s.Founder.Username));
    }

    public async Task<bool> VerifyStartupAsync(string id, StartupStatus status)
    {
        var startup = await _dbContext.Startups
            .Include(s => s.Founder)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (startup == null) return false;

        startup.Status = status;
        _unitOfWork.Startups.Update(startup);

        // Notify founder
        await _unitOfWork.Notifications.AddAsync(new Notification
        {
            UserId = startup.FounderId,
            Message = $"Your startup profile '{startup.Name}' has been {status.ToString().ToLower()} by the administration.",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleSaveStartupAsync(string startupId, string investorId)
    {
        var existing = (await _unitOfWork.SavedStartups.FindAsync(s => s.StartupId == startupId && s.InvestorId == investorId)).FirstOrDefault();

        if (existing != null)
        {
            _unitOfWork.SavedStartups.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
            return false; // Bookmark removed
        }
        else
        {
            var newSave = new SavedStartup
            {
                InvestorId = investorId,
                StartupId = startupId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.SavedStartups.AddAsync(newSave);
            await _unitOfWork.SaveChangesAsync();
            return true; // Bookmark added
        }
    }

    public async Task<IEnumerable<StartupResponse>> GetSavedStartupsAsync(string investorId)
    {
        var saves = await _dbContext.SavedStartups
            .Include(s => s.Startup)
                .ThenInclude(st => st.Founder)
            .Include(s => s.Startup)
                .ThenInclude(st => st.TeamMembers)
            .Where(s => s.InvestorId == investorId)
            .ToListAsync();

        return saves.Select(s => MapToResponse(s.Startup, s.Startup.Founder.Username));
    }

    private static StartupResponse MapToResponse(Startup startup, string founderName)
    {
        return new StartupResponse(
            startup.Id,
            startup.FounderId,
            founderName,
            startup.Name,
            startup.Tagline,
            startup.Industry,
            startup.Category,
            startup.Description,
            startup.BusinessModel,
            startup.FinancialOverview,
            startup.LogoUrl,
            startup.PitchDeckUrl,
            startup.WebsiteUrl,
            startup.VideoUrl,
            startup.Status,
            startup.HealthScore,
            startup.CreatedAt,
            startup.TeamMembers.Select(tm => new TeamMemberDto(tm.Name, tm.Role, tm.Bio, tm.AvatarUrl)).ToList()
        );
    }
}
