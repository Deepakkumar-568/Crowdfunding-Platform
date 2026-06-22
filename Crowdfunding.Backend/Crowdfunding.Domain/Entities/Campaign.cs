using System;
using System.Collections.Generic;
using Crowdfunding.Domain.Enums;

namespace Crowdfunding.Domain.Entities;

public class Campaign
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string StartupId { get; set; } = null!;
    public decimal FundingGoal { get; set; }
    public decimal CurrentFunding { get; set; } = 0.00m;
    public int InvestorCount { get; set; } = 0;
    public string Pitch { get; set; } = null!;
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Startup Startup { get; set; } = null!;
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
}
