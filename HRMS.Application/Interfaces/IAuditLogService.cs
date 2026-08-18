using HRMS.Application.Common;
using HRMS.Application.DTOs.AuditLog;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Application service contract for recording and querying business audit logs.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Record a business event in the audit log.
    /// This is fire-and-forget — call from service layer after every significant action.
    /// </summary>
    Task LogAsync(
        Guid performedByUserId,
        Guid? organizationId,
        string action,
        string entityType,
        Guid? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null);

    /// <summary>Query audit logs with optional filters. HR/Admin only.</summary>
    Task<ApiResponse<List<AuditLogResponseDto>>> GetLogsAsync(
        Guid orgId,
        string? entityType = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50);

    /// <summary>Get human-readable activity feed for dashboard.</summary>
    Task<ApiResponse<List<ActivityFeedItemDto>>> GetActivityFeedAsync(Guid orgId, int page = 1, int pageSize = 10);
}
