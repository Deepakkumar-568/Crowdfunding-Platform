using System.Collections.Generic;
using System.Threading.Tasks;
using Crowdfunding.Application.DTOs;

namespace Crowdfunding.Application.Common.Interfaces;

public interface IInvestmentService
{
    Task<InvestmentResponse?> CreateInvestmentAsync(InvestmentRequest request, string investorId);
    Task<IEnumerable<InvestmentResponse>> GetInvestmentsByInvestorIdAsync(string investorId);
    Task<IEnumerable<InvestmentResponse>> GetInvestmentsByCampaignIdAsync(string campaignId);
}
