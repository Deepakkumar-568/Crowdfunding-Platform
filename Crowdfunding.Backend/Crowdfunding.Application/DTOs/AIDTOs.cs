namespace Crowdfunding.Application.DTOs;

public record AIEnhanceDescriptionRequest(string Name, string Tagline, string Description);

public record AIImprovePitchRequest(string Name, string Pitch);

public record AIGenerateSummaryRequest(string Name, string Description, string BusinessModel, string Financials);

public record AIRecommendationRequest(string InvestorInterests);

public record AITextResponse(string Result);

public record AIPitchRefineResponse(
    bool Success,
    string GeneratedPitch,
    int Score,
    string FundingStage,
    string EstimatedValuation,
    string SuggestedFunding,
    string[] Strengths,
    string[] Weaknesses,
    string[] Risks,
    string[] InvestorQuestions
);
