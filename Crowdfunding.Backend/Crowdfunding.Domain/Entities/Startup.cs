using System;
using System.Collections.Generic;
using Crowdfunding.Domain.Enums;

namespace Crowdfunding.Domain.Entities;

public class Startup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FounderId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Tagline { get; set; } = null!;
    public string Industry { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string BusinessModel { get; set; } = null!;
    public string FinancialOverview { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? PitchDeckUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? VideoUrl { get; set; }
    public StartupStatus Status { get; set; } = StartupStatus.Pending;
    public int HealthScore { get; set; } = 70;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Founder { get; set; } = null!;
    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    public ICollection<SavedStartup> SavedByInvestors { get; set; } = new List<SavedStartup>();
}
