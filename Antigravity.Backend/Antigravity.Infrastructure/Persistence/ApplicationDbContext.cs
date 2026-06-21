using Microsoft.EntityFrameworkCore;
using Antigravity.Domain.Entities;

namespace Antigravity.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Startup> Startups => Set<Startup>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Investment> Investments => Set<Investment>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<SavedStartup> SavedStartups => Set<SavedStartup>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        // Startup configurations
        modelBuilder.Entity<Startup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Tagline).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Industry).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(d => d.Founder)
                .WithMany(p => p.Startups)
                .HasForeignKey(d => d.FounderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Campaign configurations
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FundingGoal).HasPrecision(18, 2);
            entity.Property(e => e.CurrentFunding).HasPrecision(18, 2);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(d => d.Startup)
                .WithMany(p => p.Campaigns)
                .HasForeignKey(d => d.StartupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Investment configurations
        modelBuilder.Entity<Investment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.RoiTracked).HasPrecision(5, 2);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(d => d.Investor)
                .WithMany(p => p.Investments)
                .HasForeignKey(d => d.InvestorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Campaign)
                .WithMany(p => p.Investments)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TeamMember configurations
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(100).IsRequired();

            entity.HasOne(d => d.Startup)
                .WithMany(p => p.TeamMembers)
                .HasForeignKey(d => d.StartupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SavedStartup configurations (Join Table)
        modelBuilder.Entity<SavedStartup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.InvestorId, e.StartupId }).IsUnique();

            entity.HasOne(d => d.Investor)
                .WithMany()
                .HasForeignKey(d => d.InvestorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Startup)
                .WithMany(p => p.SavedByInvestors)
                .HasForeignKey(d => d.StartupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Notification configurations
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).HasMaxLength(500).IsRequired();

            entity.HasOne(d => d.User)
                .WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
