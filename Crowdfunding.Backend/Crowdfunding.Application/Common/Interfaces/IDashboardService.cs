using System.Threading.Tasks;
using Crowdfunding.Application.DTOs;

namespace Crowdfunding.Application.Common.Interfaces;

public interface IDashboardService
{
    Task<FounderDashboardResponse> GetFounderDashboardAsync(string founderId);
    Task<InvestorDashboardResponse> GetInvestorDashboardAsync(string investorId);
    Task<AdminDashboardResponse> GetAdminDashboardAsync();
}
