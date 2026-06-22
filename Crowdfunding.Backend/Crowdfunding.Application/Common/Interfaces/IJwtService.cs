using Crowdfunding.Domain.Entities;

namespace Crowdfunding.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
