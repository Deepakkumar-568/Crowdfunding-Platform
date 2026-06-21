using System;
using System.Collections.Generic;
using Antigravity.Domain.Enums;

namespace Antigravity.Domain.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Role Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Startup> Startups { get; set; } = new List<Startup>();
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
