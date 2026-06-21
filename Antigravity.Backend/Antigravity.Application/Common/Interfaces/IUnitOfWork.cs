using System;
using System.Threading.Tasks;
using Antigravity.Domain.Entities;

namespace Antigravity.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Startup> Startups { get; }
    IRepository<Campaign> Campaigns { get; }
    IRepository<Investment> Investments { get; }
    IRepository<TeamMember> TeamMembers { get; }
    IRepository<SavedStartup> SavedStartups { get; }
    IRepository<Notification> Notifications { get; }
    Task<int> SaveChangesAsync();
}
