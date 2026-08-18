using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.Attendance;

/// <summary>
/// Request DTO for HR/Admin to manually mark or override an employee's attendance record.
/// This is used for scenarios like: marking absent for the previous day, correcting records, etc.
/// </summary>
public class MarkAttendanceDto
{
    /// <summary>Employee ID whose attendance is being marked.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>The date for which attendance is being marked.</summary>
    public DateOnly Date { get; set; }

    /// <summary>The attendance status to set.</summary>
    public AttendanceStatus Status { get; set; }

    /// <summary>Optional override check-in time (UTC). Leave null to keep existing or skip for Absent.</summary>
    public DateTime? CheckIn { get; set; }

    /// <summary>Optional override check-out time (UTC). Leave null if not applicable.</summary>
    public DateTime? CheckOut { get; set; }

    /// <summary>Optional notes explaining the override (e.g., "Work from home approved").</summary>
    public string? Notes { get; set; }
}
