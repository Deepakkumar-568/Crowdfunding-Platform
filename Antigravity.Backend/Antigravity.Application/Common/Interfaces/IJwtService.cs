using Antigravity.Domain.Entities;

namespace Antigravity.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
