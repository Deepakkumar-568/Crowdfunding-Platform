using System;

namespace Antigravity.Domain.Entities;

public class SavedStartup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InvestorId { get; set; } = null!;
    public string StartupId { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Investor { get; set; } = null!;
    public Startup Startup { get; set; } = null!;
}
