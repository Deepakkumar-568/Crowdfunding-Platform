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

public class InvestmentService : IInvestmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _dbContext;

    public InvestmentService(IUnitOfWork unitOfWork, ApplicationDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<InvestmentResponse?> CreateInvestmentAsync(InvestmentRequest request, string investorId)
    {
        var campaign = await _dbContext.Campaigns
            .Include(c => c.Startup)
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId);

        if (campaign == null) return null;

        if (campaign.Status != CampaignStatus.Active)
        {
            throw new InvalidOperationException("Investments can only be processed on active fundraising campaigns.");
        }

        var investor = await _unitOfWork.Users.GetByIdAsync(investorId)
            ?? throw new ArgumentException("Investor profile not found.");

        // Calculate a mocked ROI (e.g. 5% - 25% return, simulated)
        var random = new Random();
        var roi = (decimal)(random.NextDouble() * 20.0 + 5.0);

        var investment = new Investment
        {
            InvestorId = investorId,
            CampaignId = request.CampaignId,
            Amount = request.Amount,
            RoiTracked = roi,
            InvestedAt = DateTime.UtcNow,
            Status = InvestmentStatus.Success
        };

        // Update campaign funding progress
        campaign.CurrentFunding += request.Amount;
        
        // Increment unique investor count if this is the investor's first investment in this campaign
        var alreadyInvested = await _dbContext.Investments
            .AnyAsync(i => i.CampaignId == request.CampaignId && i.InvestorId == investorId);

        if (!alreadyInvested)
        {
            campaign.InvestorCount += 1;
        }

        await _unitOfWork.Investments.AddAsync(investment);
        _unitOfWork.Campaigns.Update(campaign);

        // Notify investor
        await _unitOfWork.Notifications.AddAsync(new Notification
        {
            UserId = investorId,
            Message = $"Success! You have invested ${request.Amount:N0} in '{campaign.Startup.Name}'. Thank you for your support.",
            CreatedAt = DateTime.UtcNow
        });

        // Notify founder
        await _unitOfWork.Notifications.AddAsync(new Notification
        {
            UserId = campaign.Startup.FounderId,
            Message = $"Great news! An investor has contributed ${request.Amount:N0} to your campaign for '{campaign.Startup.Name}'.",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(investment, campaign, campaign.Startup);
    }

    public async Task<IEnumerable<InvestmentResponse>> GetInvestmentsByInvestorIdAsync(string investorId)
    {
        var investments = await _dbContext.Investments
            .Include(i => i.Campaign)
                .ThenInclude(c => c.Startup)
            .Where(i => i.InvestorId == investorId)
            .OrderByDescending(i => i.InvestedAt)
            .ToListAsync();

        return investments.Select(i => MapToResponse(i, i.Campaign, i.Campaign.Startup));
    }

    public async Task<IEnumerable<InvestmentResponse>> GetInvestmentsByCampaignIdAsync(string campaignId)
    {
        var investments = await _dbContext.Investments
            .Include(i => i.Campaign)
                .ThenInclude(c => c.Startup)
            .Where(i => i.CampaignId == campaignId)
            .OrderByDescending(i => i.InvestedAt)
            .ToListAsync();

        return investments.Select(i => MapToResponse(i, i.Campaign, i.Campaign.Startup));
    }

    private static InvestmentResponse MapToResponse(Investment investment, Campaign campaign, Startup startup)
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
