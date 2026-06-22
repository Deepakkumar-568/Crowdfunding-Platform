using System;
using System.Collections.Generic;
using Crowdfunding.Domain.Enums;

namespace Crowdfunding.Application.DTOs;

public record TeamMemberDto(string Name, string Role, string Bio, string? AvatarUrl);

public record StartupCreateRequest(
    string Name,
    string Tagline,
    string Industry,
    string Category,
    string Description,
    string BusinessModel,
    string FinancialOverview,
    string? LogoUrl,
    string? PitchDeckUrl,
    string? WebsiteUrl,
    string? VideoUrl,
    List<TeamMemberDto> TeamMembers
);

public record StartupResponse(
    string Id,
    string FounderId,
    string FounderName,
    string Name,
    string Tagline,
    string Industry,
    string Category,
    string Description,
    string BusinessModel,
    string FinancialOverview,
    string? LogoUrl,
    string? PitchDeckUrl,
    string? WebsiteUrl,
    string? VideoUrl,
    StartupStatus Status,
    int HealthScore,
    DateTime CreatedAt,
    List<TeamMemberDto> TeamMembers
);

public record StartupVerificationRequest(StartupStatus Status);
