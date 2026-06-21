using System;
using Antigravity.Domain.Enums;

namespace Antigravity.Application.DTOs;

public record CampaignCreateRequest(
    string StartupId,
    decimal FundingGoal,
    string Pitch,
    DateTime StartsAt,
    DateTime EndsAt
);

public record CampaignResponse(
    string Id,
    string StartupId,
    string StartupName,
    string StartupTagline,
    string StartupLogoUrl,
    decimal FundingGoal,
    decimal CurrentFunding,
    int InvestorCount,
    string Pitch,
    CampaignStatus Status,
    DateTime StartsAt,
    DateTime EndsAt,
    int DaysLeft,
    int ProgressPercentage
);

public record CampaignApprovalRequest(CampaignStatus Status);
