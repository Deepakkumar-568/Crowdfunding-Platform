using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Antigravity.Domain.Entities;
using Antigravity.Domain.Enums;

namespace Antigravity.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Users
        if (!context.Users.Any())
        {
            var admin = new User
            {
                Username = "admin",
                Email = "admin@crowdfunding.com",
                PasswordHash = HashPassword("Password123"),
                Role = Role.Admin,
                CreatedAt = DateTime.UtcNow
            };

            var founder = new User
            {
                Username = "founder",
                Email = "founder@crowdfunding.com",
                PasswordHash = HashPassword("Password123"),
                Role = Role.Founder,
                CreatedAt = DateTime.UtcNow
            };

            var investor = new User
            {
                Username = "investor",
                Email = "investor@crowdfunding.com",
                PasswordHash = HashPassword("Password123"),
                Role = Role.Investor,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(admin, founder, investor);
            await context.SaveChangesAsync();

            // 2. Seed Startups
            var startup1 = new Startup
            {
                FounderId = founder.Id,
                Name = "Quantum Fusion",
                Tagline = "Clean Energy for the Next Century",
                Category = "Energy",
                Industry = "HardTech",
                Description = "Developing superconducting tokamak reactors to bring cheap, clean, and commercial fusion power to the grid. Backed by cutting edge research in high-temperature superconductor magnets.",
                BusinessModel = "B2B energy utility partnerships and long term energy supply contracts with industrial grids.",
                FinancialOverview = "Targeting pilot reactor completion by 2028. Projecting profitability by 2030 with early test revenues.",
                LogoUrl = "https://images.unsplash.com/photo-1507413245164-6160d8298b31?auto=format&fit=crop&w=200&h=200&q=80",
                WebsiteUrl = "https://quantumfusion.io",
                PitchDeckUrl = "https://quantumfusion.io/pitch.pdf",
                Status = StartupStatus.Approved,
                HealthScore = 92,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            };

            var startup2 = new Startup
            {
                FounderId = founder.Id,
                Name = "ZeroGravity Logistics",
                Tagline = "Suborbital Autonomous Drone Delivery",
                Category = "Aerospace",
                Industry = "Logistics",
                Description = "Reimagining regional middle-mile shipping with hydrogen-powered autonomous VTOL suborbital cargo drones operating at high speeds.",
                BusinessModel = "Subscription-based logistics routing and direct contract logistics fees for urgent freight.",
                FinancialOverview = "Prototype flight tests completed. Proposing commercial regulatory filing in late 2026.",
                LogoUrl = "https://images.unsplash.com/photo-1517976487492-5750f3195933?auto=format&fit=crop&w=200&h=200&q=80",
                WebsiteUrl = "https://zerogravity.delivery",
                Status = StartupStatus.Pending,
                HealthScore = 78,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            await context.Startups.AddRangeAsync(startup1, startup2);
            await context.SaveChangesAsync();

            // Seed Team Members
            var team1 = new TeamMember
            {
                StartupId = startup1.Id,
                Name = "Dr. Elena Rostova",
                Role = "Chief Fusion Officer",
                Bio = "Former MIT Plasma Science and Fusion Center researcher. PhD in Nuclear Engineering.",
                AvatarUrl = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=150&h=150&q=80"
            };

            var team2 = new TeamMember
            {
                StartupId = startup2.Id,
                Name = "Marcus Vance",
                Role = "Founder & CEO",
                Bio = "Aerospace structures engineer, formerly SpaceX propulsion division lead.",
                AvatarUrl = "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=150&h=150&q=80"
            };

            await context.TeamMembers.AddRangeAsync(team1, team2);
            await context.SaveChangesAsync();

            // 3. Seed Campaigns
            var campaign1 = new Campaign
            {
                StartupId = startup1.Id,
                FundingGoal = 1500000.00m,
                CurrentFunding = 425000.00m,
                InvestorCount = 14,
                Pitch = "Superconducting tokamaks offer 10x magnetic confinement efficiency, reducing reactor size and capital costs by 80%. We are raising seed capital to complete our phase 2 coils testing.",
                Status = CampaignStatus.Active,
                StartsAt = DateTime.UtcNow.AddDays(-10),
                EndsAt = DateTime.UtcNow.AddDays(50),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            var campaign2 = new Campaign
            {
                StartupId = startup2.Id,
                FundingGoal = 800000.00m,
                CurrentFunding = 0.00m,
                InvestorCount = 0,
                Pitch = "Middle-mile freight shipping is slower than ever due to urban road congestion. Our suborbital cargo drones bypass land routes entirely, delivering freight 10x faster with zero carbon emissions.",
                Status = CampaignStatus.Draft,
                StartsAt = DateTime.UtcNow.AddDays(10),
                EndsAt = DateTime.UtcNow.AddDays(40),
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            await context.Campaigns.AddRangeAsync(campaign1, campaign2);
            await context.SaveChangesAsync();

            // Seed investments in active campaign
            var investment = new Investment
            {
                InvestorId = investor.Id,
                CampaignId = campaign1.Id,
                Amount = 50000.00m,
                RoiTracked = 18.5m,
                InvestedAt = DateTime.UtcNow.AddDays(-2),
                Status = InvestmentStatus.Success
            };
            
            await context.Investments.AddAsync(investment);

            // Update active campaign with the investment
            campaign1.CurrentFunding += 50000.00m;
            campaign1.InvestorCount += 1;

            // Notifications
            await context.Notifications.AddAsync(new Notification
            {
                UserId = founder.Id,
                Message = $"Welcome to Crowdfunding Platform, founder! You have successfully submitted 'Quantum Fusion' and 'ZeroGravity Logistics'.",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            });

            await context.Notifications.AddAsync(new Notification
            {
                UserId = investor.Id,
                Message = $"Your investment of $50,000 in 'Quantum Fusion Seed Round' was processed successfully.",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            });

            await context.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
