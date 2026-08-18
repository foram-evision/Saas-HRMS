using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Data access contract for business audit logs.
/// Implemented by AuditLogRepository in the Infrastructure layer.
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);

    /// <summary>Get audit logs for an organization, optionally filtered by entity type and date range.</summary>
    Task<IEnumerable<AuditLog>> GetAsync(
        Guid organizationId,
        string? entityType = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50);

    /// <summary>Get the N most recent audit log entries for the dashboard activity feed.</summary>
    Task<IEnumerable<AuditLog>> GetRecentAsync(Guid organizationId, int count = 10);
}
