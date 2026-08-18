using System.Security.Claims;
using HRMS.Application.Common;
using HRMS.Application.DTOs.AuditLog;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Query business audit trail for organization. Admin/HR only.
    /// Supports filtering by entityType, from date, to date, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<List<AuditLogResponseDto>>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _auditLogService.GetLogsAsync(orgId.Value, entityType, from, to, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a human-readable activity feed for the dashboard.
    /// Supports pagination. Admin/HR only.
    /// </summary>
    [HttpGet("activity-feed")]
    public async Task<IActionResult> GetActivityFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<List<ActivityFeedItemDto>>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _auditLogService.GetActivityFeedAsync(orgId.Value, page, pageSize);
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
