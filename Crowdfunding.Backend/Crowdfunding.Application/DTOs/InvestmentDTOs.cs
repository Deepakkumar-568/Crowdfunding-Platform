using System;
using Crowdfunding.Domain.Enums;

namespace Crowdfunding.Application.DTOs;

public record InvestmentRequest(string CampaignId, decimal Amount);

public record InvestmentResponse(
    string Id,
    string CampaignId,
    string CampaignName,
    string StartupId,
    string StartupName,
    string StartupLogoUrl,
    decimal Amount,
    decimal RoiTracked,
    DateTime InvestedAt,
    InvestmentStatus Status
);
