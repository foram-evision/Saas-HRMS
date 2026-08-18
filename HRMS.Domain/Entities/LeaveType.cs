using HRMS.Domain.Base;

namespace HRMS.Domain.Entities;

/// <summary>
/// Defines a category of leave available in an organization (e.g., Casual, Sick, Paid).
/// Each organization can configure its own set of leave types with their allowed days.
/// </summary>
public class LeaveType : BaseEntity
{
    /// <summary>Display name of this leave type (e.g., "Sick Leave", "Casual Leave").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description about when this leave type can be used.</summary>
    public string? Description { get; set; }

    /// <summary>Maximum number of days allowed per year for this leave type.</summary>
    public int TotalDaysAllowed { get; set; }

    /// <summary>Organization this leave type belongs to (tenant-scoped).</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Indicates whether this leave type is currently active and can be applied.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>UTC timestamp when this record was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ─────────────────────────────────────────────────

    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
