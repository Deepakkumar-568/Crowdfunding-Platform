using System;
using System.Threading.Tasks;
using Crowdfunding.Application.Common.Interfaces;
using Crowdfunding.Domain.Entities;
using Crowdfunding.Infrastructure.Persistence;

namespace Crowdfunding.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    private IRepository<User>? _users;
    private IRepository<Startup>? _startups;
    private IRepository<Campaign>? _campaigns;
    private IRepository<Investment>? _investments;
    private IRepository<TeamMember>? _teamMembers;
    private IRepository<SavedStartup>? _savedStartups;
    private IRepository<Notification>? _notifications;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Startup> Startups => _startups ??= new Repository<Startup>(_context);
    public IRepository<Campaign> Campaigns => _campaigns ??= new Repository<Campaign>(_context);
    public IRepository<Investment> Investments => _investments ??= new Repository<Investment>(_context);
    public IRepository<TeamMember> TeamMembers => _teamMembers ??= new Repository<TeamMember>(_context);
    public IRepository<SavedStartup> SavedStartups => _savedStartups ??= new Repository<SavedStartup>(_context);
    public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
