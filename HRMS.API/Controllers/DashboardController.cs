using System.Security.Claims;
using HRMS.Application.Common;
using HRMS.Application.DTOs.Dashboard;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get full dashboard metrics summary for the authenticated user's organization.
    /// Includes employee counts, today's attendance, pending leaves, monthly payroll costs, and recent activity.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<DashboardDto>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _dashboardService.GetDashboardAsync(orgId.Value);
        return Ok(result);
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirstValue("organizationId")
                 ?? User.FindFirstValue("OrganizationId")
                 ?? User.FindFirstValue("org_id");

        if (Guid.TryParse(claim, out var orgId) && orgId != Guid.Empty)
            return orgId;

        return null;
    }
}
