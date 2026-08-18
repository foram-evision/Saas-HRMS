using HRMS.Domain.Base;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

/// <summary>
/// Records daily attendance for an employee.
/// One record per employee per calendar date.
/// CheckIn and CheckOut are stored as UTC DateTime for precision.
/// TotalHours is auto-calculated when the employee checks out.
/// </summary>
public class Attendance : BaseEntity
{
    /// <summary>The employee whose attendance this record belongs to.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Organization this attendance record belongs to (for multi-tenant scoping).</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>The calendar date of this attendance record (date only, no time).</summary>
    public DateOnly Date { get; set; }

    /// <summary>UTC timestamp when the employee checked in. Null if not yet checked in.</summary>
    public DateTime? CheckIn { get; set; }

    /// <summary>UTC timestamp when the employee checked out. Null if not yet checked out.</summary>
    public DateTime? CheckOut { get; set; }

    /// <summary>
    /// Total working hours calculated as (CheckOut - CheckIn).
    /// Null until the employee checks out.
    /// </summary>
    public decimal? TotalHours { get; set; }

    /// <summary>Current attendance status for this record.</summary>
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Absent;

    /// <summary>Optional notes from HR or the employee (e.g., "Working from home").</summary>
    public string? Notes { get; set; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when this record was last modified.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ─────────────────────────────────────────────────

    public virtual Employee Employee { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
}
