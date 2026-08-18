using HRMS.Application.Common;
using HRMS.Application.DTOs.Dashboard;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Application service contract for the HRMS dashboard analytics.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Aggregate all key HRMS metrics for the given organization.
    /// Data is scoped strictly to the organization — no cross-org data leaks.
    /// </summary>
    Task<ApiResponse<DashboardDto>> GetDashboardAsync(Guid orgId);
}
