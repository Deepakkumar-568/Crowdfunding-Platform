using System.Collections.Generic;
using System.Threading.Tasks;
using Antigravity.Application.DTOs;
using Antigravity.Domain.Enums;

namespace Antigravity.Application.Common.Interfaces;

public interface ICampaignService
{
    Task<CampaignResponse> CreateCampaignAsync(CampaignCreateRequest request);
    Task<CampaignResponse?> GetCampaignByIdAsync(string id);
    Task<IEnumerable<CampaignResponse>> GetAllCampaignsAsync(string? search, decimal? minFundingGoal, decimal? maxFundingGoal, string? sortBy);
    Task<bool> ApproveCampaignAsync(string id, CampaignStatus status);
    Task<IEnumerable<CampaignResponse>> GetCampaignsByStartupIdAsync(string startupId);
}
