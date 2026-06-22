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

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FounderDashboardResponse> GetFounderDashboardAsync(string founderId)
    {
        var startups = await _dbContext.Startups
            .Where(s => s.FounderId == founderId)
            .ToListAsync();

        var startupIds = startups.Select(s => s.Id).ToList();

        var campaigns = await _dbContext.Campaigns
            .Include(c => c.Startup)
            .Where(c => startupIds.Contains(c.StartupId))
            .ToListAsync();

        var campaignIds = campaigns.Select(c => c.Id).ToList();

        var investments = await _dbContext.Investments
            .Include(i => i.Campaign)
                .ThenInclude(c => c.Startup)
            .Where(i => campaignIds.Contains(i.CampaignId))
            .ToListAsync();

        var notifications = await _dbContext.Notifications
            .Where(n => n.UserId == founderId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new NotificationDto(n.Id, n.Message, n.IsRead, n.CreatedAt))
            .ToListAsync();

        // Calculate metrics
        var totalRaised = campaigns.Sum(c => c.CurrentFunding);
        var activeCampaignCount = campaigns.Count(c => c.Status == CampaignStatus.Active);
        var investorCount = campaigns.Sum(c => c.InvestorCount);
        var averageHealth = startups.Any() ? (int)startups.Average(s => s.HealthScore) : 70;

        var cards = new List<MetricCardDto>
        {
            new MetricCardDto("Total Funds Raised", $"${totalRaised:N0}", "+12.4% from last month", true),
            new MetricCardDto("Active Campaigns", activeCampaignCount.ToString(), "Currently raising capital", true),
            new MetricCardDto("Total Supporters", investorCount.ToString(), "+5 new this week", true),
            new MetricCardDto("Startup Health Index", $"{averageHealth}/100", "AI Evaluated", true)
        };

        var mappedCampaigns = campaigns
            .Select(c => MapCampaignToResponse(c, c.Startup))
            .ToList();

        var mappedInvestments = investments
            .OrderByDescending(i => i.InvestedAt)
            .Take(5)
            .Select(i => MapInvestmentToResponse(i, i.Campaign, i.Campaign.Startup))
            .ToList();

        return new FounderDashboardResponse(cards, mappedCampaigns, mappedInvestments, notifications, averageHealth);
    }

    public async Task<InvestorDashboardResponse> GetInvestorDashboardAsync(string investorId)
    {
        var investments = await _dbContext.Investments
            .Include(i => i.Campaign)
                .ThenInclude(c => c.Startup)
            .Where(i => i.InvestorId == investorId)
            .ToListAsync();

        var savedStartups = await _dbContext.SavedStartups
            .Include(s => s.Startup)
                .ThenInclude(st => st.Founder)
            .Include(s => s.Startup)
                .ThenInclude(st => st.TeamMembers)
            .Where(s => s.InvestorId == investorId)
            .Select(s => MapStartupToResponse(s.Startup, s.Startup.Founder.Username))
            .ToListAsync();

        var notifications = await _dbContext.Notifications
            .Where(n => n.UserId == investorId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new NotificationDto(n.Id, n.Message, n.IsRead, n.CreatedAt))
            .ToListAsync();

        // Calculate metrics
        var totalInvested = investments.Sum(i => i.Amount);
        var portfolioValue = investments.Sum(i => i.Amount * (1 + (i.RoiTracked / 100)));
        var activeCount = investments.Count(i => i.Campaign.Status == CampaignStatus.Active);
        var averageRoi = investments.Any() ? investments.Average(i => i.RoiTracked) : 0.00m;

        var cards = new List<MetricCardDto>
        {
            new MetricCardDto("Total Capital Invested", $"${totalInvested:N0}", "Across all seed rounds", true),
            new MetricCardDto("Est. Portfolio Value", $"${portfolioValue:N2}", $"+${(portfolioValue - totalInvested):N2} ROI", true),
            new MetricCardDto("Active Portfolios", activeCount.ToString(), "Campaigns in-progress", true),
            new MetricCardDto("Average Tracked ROI", $"{averageRoi:N2}%", "Calculated yield", true)
        };

        var mappedInvestments = investments
            .Select(i => MapInvestmentToResponse(i, i.Campaign, i.Campaign.Startup))
            .ToList();

        var activeInvestments = mappedInvestments
            .Where(i => i.Status == InvestmentStatus.Success && _dbContext.Campaigns.Any(c => c.Id == i.CampaignId && c.Status == CampaignStatus.Active))
            .ToList();

        var recentHistory = mappedInvestments
            .OrderByDescending(i => i.InvestedAt)
            .Take(5)
            .ToList();

        return new InvestorDashboardResponse(cards, activeInvestments, savedStartups, recentHistory, notifications);
    }

    public async Task<AdminDashboardResponse> GetAdminDashboardAsync()
    {
        var userCount = await _dbContext.Users.CountAsync();
        var startupCount = await _dbContext.Startups.CountAsync();
        var investmentCount = await _dbContext.Investments.CountAsync();
        var totalRaised = await _dbContext.Campaigns.SumAsync(c => c.CurrentFunding);

        var pendingStartups = await _dbContext.Startups
            .Include(s => s.Founder)
            .Include(s => s.TeamMembers)
            .Where(s => s.Status == StartupStatus.Pending)
            .Select(s => MapStartupToResponse(s, s.Founder.Username))
            .ToListAsync();

        var pendingCampaigns = await _dbContext.Campaigns
            .Include(c => c.Startup)
            .Where(c => c.Status == CampaignStatus.Draft)
            .Select(c => MapCampaignToResponse(c, c.Startup))
            .ToListAsync();

        var recentActivityLog = await _dbContext.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(8)
            .Select(n => new NotificationDto(n.Id, n.Message, n.IsRead, n.CreatedAt))
            .ToListAsync();

        var cards = new List<MetricCardDto>
        {
            new MetricCardDto("Total Platform Users", userCount.ToString(), "Founders & Investors", true),
            new MetricCardDto("Total Startups", startupCount.ToString(), "Registered organizations", true),
            new MetricCardDto("Processed Transactions", investmentCount.ToString(), "Capital contracts", true),
            new MetricCardDto("Total Capital Channelled", $"${totalRaised:N0}", "Platform transactional volume", true)
        };

        return new AdminDashboardResponse(cards, pendingStartups, pendingCampaigns, recentActivityLog);
    }

    private static StartupResponse MapStartupToResponse(Startup startup, string founderName)
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

    private static CampaignResponse MapCampaignToResponse(Campaign campaign, Startup startup)
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

    private static InvestmentResponse MapInvestmentToResponse(Investment investment, Campaign campaign, Startup startup)
    {
        return new InvestmentResponse(
            investment.Id,
            investment.CampaignId,
            $"{startup.Name} Seed Round",
            startup.Id,
            startup.Name,
            startup.LogoUrl ?? "",
            investment.Amount,
            investment.RoiTracked,
            investment.InvestedAt,
            investment.Status
        );
    }
}
