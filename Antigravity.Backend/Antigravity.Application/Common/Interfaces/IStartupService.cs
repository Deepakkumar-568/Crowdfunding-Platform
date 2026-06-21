using System.Collections.Generic;
using System.Threading.Tasks;
using Antigravity.Application.DTOs;
using Antigravity.Domain.Enums;

namespace Antigravity.Application.Common.Interfaces;

public interface IStartupService
{
    Task<StartupResponse> CreateStartupAsync(StartupCreateRequest request, string founderId);
    Task<StartupResponse?> GetStartupByIdAsync(string id);
    Task<IEnumerable<StartupResponse>> GetAllStartupsAsync(string? search, string? category, string? industry, string? sortBy);
    Task<bool> VerifyStartupAsync(string id, StartupStatus status);
    Task<bool> ToggleSaveStartupAsync(string startupId, string investorId);
    Task<IEnumerable<StartupResponse>> GetSavedStartupsAsync(string investorId);
}
