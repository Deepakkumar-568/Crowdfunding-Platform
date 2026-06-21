using System.Threading.Tasks;
using Antigravity.Application.DTOs;

namespace Antigravity.Application.Common.Interfaces;

public interface IDashboardService
{
    Task<FounderDashboardResponse> GetFounderDashboardAsync(string founderId);
    Task<InvestorDashboardResponse> GetInvestorDashboardAsync(string investorId);
    Task<AdminDashboardResponse> GetAdminDashboardAsync();
}
