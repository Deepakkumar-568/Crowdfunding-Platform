using System.Collections.Generic;
using System.Threading.Tasks;
using Antigravity.Application.DTOs;

namespace Antigravity.Application.Common.Interfaces;

public interface IInvestmentService
{
    Task<InvestmentResponse?> CreateInvestmentAsync(InvestmentRequest request, string investorId);
    Task<IEnumerable<InvestmentResponse>> GetInvestmentsByInvestorIdAsync(string investorId);
    Task<IEnumerable<InvestmentResponse>> GetInvestmentsByCampaignIdAsync(string campaignId);
}
