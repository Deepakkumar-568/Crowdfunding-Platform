using System.Threading.Tasks;
using Crowdfunding.Application.DTOs;

namespace Crowdfunding.Application.Common.Interfaces;

public interface IOllamaService
{
    Task<string> EnhanceDescriptionAsync(string name, string tagline, string description);
    Task<AIPitchRefineResponse> ImprovePitchAsync(string name, string pitch);
    Task<string> GenerateSummaryAsync(string name, string description, string businessModel, string financials);
    Task<string> GetInvestorRecommendationsAsync(string investorInterests, string startupListJson);
    Task<int> CalculateHealthScoreAsync(string name, decimal fundingGoal, decimal currentFunding, int teamCount);
}
