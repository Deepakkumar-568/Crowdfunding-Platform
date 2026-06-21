namespace Antigravity.Application.DTOs;

public record AIEnhanceDescriptionRequest(string Name, string Tagline, string Description);

public record AIImprovePitchRequest(string Name, string Pitch);

public record AIGenerateSummaryRequest(string Name, string Description, string BusinessModel, string Financials);

public record AIRecommendationRequest(string InvestorInterests);

public record AITextResponse(string Result);
