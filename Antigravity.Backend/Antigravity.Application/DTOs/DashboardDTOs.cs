using System.Collections.Generic;

namespace Antigravity.Application.DTOs;

public record MetricCardDto(string Title, string Value, string ChangeText, bool IsPositive);

public record FounderDashboardResponse(
    List<MetricCardDto> Cards,
    List<CampaignResponse> ActiveCampaigns,
    List<InvestmentResponse> RecentInvestments,
    List<NotificationDto> Notifications,
    int HealthScore
);

public record InvestorDashboardResponse(
    List<MetricCardDto> Cards,
    List<InvestmentResponse> ActiveInvestments,
    List<StartupResponse> SavedStartups,
    List<InvestmentResponse> RecentHistory,
    List<NotificationDto> Notifications
);

public record AdminDashboardResponse(
    List<MetricCardDto> Cards,
    List<StartupResponse> PendingStartups,
    List<CampaignResponse> PendingCampaigns,
    List<NotificationDto> RecentActivityLog
);

public record NotificationDto(string Id, string Message, bool IsRead, System.DateTime CreatedAt);
