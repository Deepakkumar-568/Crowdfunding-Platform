using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Antigravity.Application.Common.Interfaces;
using Antigravity.Application.DTOs;
using Antigravity.Domain.Entities;
using Antigravity.Domain.Enums;
using Antigravity.Infrastructure.Persistence;

namespace Antigravity.Infrastructure.Services;

public class CampaignService : ICampaignService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _dbContext;

    public CampaignService(IUnitOfWork unitOfWork, ApplicationDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<CampaignResponse> CreateCampaignAsync(CampaignCreateRequest request)
    {
        var startup = await _unitOfWork.Startups.GetByIdAsync(request.StartupId)
            ?? throw new ArgumentException("Startup profile not found.");

        if (startup.Status != StartupStatus.Approved)
        {
            throw new InvalidOperationException("Campaigns can only be created for verified and approved startups.");
        }

        var campaign = new Campaign
        {
            StartupId = request.StartupId,
            FundingGoal = request.FundingGoal,
            Pitch = request.Pitch,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Status = CampaignStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Campaigns.AddAsync(campaign);

        // Notify Admins
        var admins = await _unitOfWork.Users.FindAsync(u => u.Role == Role.Admin);
        foreach (var admin in admins)
        {
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = admin.Id,
                Message = $"Startup '{startup.Name}' has created a new crowdfunding campaign with a goal of ${campaign.FundingGoal:N0}.",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();
        return MapToResponse(campaign, startup);
    }

    public async Task<CampaignResponse?> GetCampaignByIdAsync(string id)
    {
        var campaign = await _dbContext.Campaigns
            .Include(c => c.Startup)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return null;

        return MapToResponse(campaign, campaign.Startup);
    }

    public async Task<IEnumerable<CampaignResponse>> GetAllCampaignsAsync(string? search, decimal? minFundingGoal, decimal? maxFundingGoal, string? sortBy)
    {
        var query = _dbContext.Campaigns
            .Include(c => c.Startup)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c => c.Startup.Name.ToLower().Contains(searchLower) || 
                                     c.Pitch.ToLower().Contains(searchLower));
        }

        if (minFundingGoal.HasValue)
        {
            query = query.Where(c => c.FundingGoal >= minFundingGoal.Value);
        }

        if (maxFundingGoal.HasValue)
        {
            query = query.Where(c => c.FundingGoal <= maxFundingGoal.Value);
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "fundingraised" => query.OrderByDescending(c => c.CurrentFunding),
            "progress" => query.OrderByDescending(c => (c.CurrentFunding / c.FundingGoal)),
            "newest" => query.OrderByDescending(c => c.CreatedAt),
            "goal" => query.OrderByDescending(c => c.FundingGoal),
            _ => query.OrderByDescending(c => c.Status == CampaignStatus.Active).ThenByDescending(c => c.CreatedAt)
        };

        var campaigns = await query.ToListAsync();
        return campaigns.Select(c => MapToResponse(c, c.Startup));
    }

    public async Task<bool> ApproveCampaignAsync(string id, CampaignStatus status)
    {
        var campaign = await _dbContext.Campaigns
            .Include(c => c.Startup)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return false;

        campaign.Status = status;
        _unitOfWork.Campaigns.Update(campaign);

        // Notify founder
        await _unitOfWork.Notifications.AddAsync(new Notification
        {
            UserId = campaign.Startup.FounderId,
            Message = $"Your campaign for '{campaign.Startup.Name}' has been updated to '{status.ToString()}'.",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CampaignResponse>> GetCampaignsByStartupIdAsync(string startupId)
    {
        var startup = await _unitOfWork.Startups.GetByIdAsync(startupId)
            ?? throw new ArgumentException("Startup profile not found.");

        var campaigns = await _dbContext.Campaigns
            .Include(c => c.Startup)
            .Where(c => c.StartupId == startupId)
            .ToListAsync();

        return campaigns.Select(c => MapToResponse(c, startup));
    }

    private static CampaignResponse MapToResponse(Campaign campaign, Startup startup)
    {
        var daysLeft = (campaign.EndsAt - DateTime.UtcNow).Days;
        daysLeft = Math.Max(daysLeft, 0);

        var progressPercentage = campaign.FundingGoal > 0 
            ? (int)Math.Round((campaign.CurrentFunding / campaign.FundingGoal) * 100) 
            : 0;

        return new CampaignResponse(
            campaign.Id,
            campaign.StartupId,
            startup.Name,
            startup.Tagline,
            startup.LogoUrl ?? "",
            campaign.FundingGoal,
            campaign.CurrentFunding,
            campaign.InvestorCount,
            campaign.Pitch,
            campaign.Status,
            campaign.StartsAt,
            campaign.EndsAt,
            daysLeft,
            progressPercentage
        );
    }
}
