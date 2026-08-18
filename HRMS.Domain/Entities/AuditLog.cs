using HRMS.Domain.Base;

namespace HRMS.Domain.Entities;

/// <summary>
/// Business-level audit log for tracking all significant HRMS operations.
/// This is separate from AuthAuditLog (which only tracks authentication events).
/// Tracks: Employee Created/Updated/Deleted, Leave Approved/Rejected,
/// Salary Generated/Paid, Department changes, etc.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>Organization context for multi-tenant scoping.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>User ID of the person who performed this action. Null for system actions.</summary>
    public Guid? PerformedByUserId { get; set; }

    /// <summary>
    /// Action that was performed (e.g., "EmployeeCreated", "LeaveApproved",
    /// "SalaryGenerated"). Use AuditEvents constants.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Type of the entity this action was performed on (e.g., "Employee", "LeaveRequest").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>ID of the specific entity that was affected.</summary>
    public Guid? EntityId { get; set; }

    /// <summary>JSON-serialized snapshot of the entity state BEFORE the change. Null for Create actions.</summary>
    public string? OldValues { get; set; }

    /// <summary>JSON-serialized snapshot of the entity state AFTER the change. Null for Delete actions.</summary>
    public string? NewValues { get; set; }

    /// <summary>IP address of the requester at the time of the action.</summary>
    public string? IpAddress { get; set; }

    /// <summary>UTC timestamp when this action occurred.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ─────────────────────────────────────────────────

    public virtual ApplicationUser? PerformedByUser { get; set; }
    public virtual Organization? Organization { get; set; }
}
