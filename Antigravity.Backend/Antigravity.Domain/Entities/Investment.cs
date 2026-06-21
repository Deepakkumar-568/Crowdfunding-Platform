using System;
using Antigravity.Domain.Enums;

namespace Antigravity.Domain.Entities;

public class Investment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InvestorId { get; set; } = null!;
    public string CampaignId { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal RoiTracked { get; set; } = 0.00m;
    public DateTime InvestedAt { get; set; } = DateTime.UtcNow;
    public InvestmentStatus Status { get; set; } = InvestmentStatus.Success;

    // Navigation properties
    public User Investor { get; set; } = null!;
    public Campaign Campaign { get; set; } = null!;
}
