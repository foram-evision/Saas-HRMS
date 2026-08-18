using HRMS.Domain.Base;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

/// <summary>
/// Represents a leave application submitted by an employee.
/// Follows a workflow: Pending → Approved | Rejected, or Cancelled by the employee.
/// </summary>
public class LeaveRequest : BaseEntity
{
    /// <summary>Employee who submitted this leave request.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Organization context for multi-tenant scoping.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>The type of leave being requested (Casual, Sick, Paid, etc.).</summary>
    public Guid LeaveTypeId { get; set; }

    /// <summary>Start date of the leave period (inclusive).</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>End date of the leave period (inclusive).</summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Total number of calendar days for this leave request.
    /// Calculated as (EndDate - StartDate).Days + 1.
    /// </summary>
    public int TotalDays { get; set; }

    /// <summary>Employee's reason for taking leave.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Current approval status of this leave request.</summary>
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    /// <summary>User ID of the HR/Admin who approved this request. Null if not yet approved.</summary>
    public Guid? ApprovedByUserId { get; set; }

    /// <summary>Reason provided by HR/Admin when rejecting the request.</summary>
    public string? RejectionReason { get; set; }

    /// <summary>UTC timestamp when the leave was approved or rejected.</summary>
    public DateTime? ActionedAt { get; set; }

    /// <summary>UTC timestamp when the leave request was submitted.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when the record was last modified.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ─────────────────────────────────────────────────

    public virtual Employee Employee { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual LeaveType LeaveType { get; set; } = null!;
    public virtual ApplicationUser? ApprovedByUser { get; set; }
}
