using System;

namespace Antigravity.Domain.Entities;

public class TeamMember
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string StartupId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Bio { get; set; } = null!;
    public string? AvatarUrl { get; set; }

    // Navigation property
    public Startup Startup { get; set; } = null!;
}
