namespace HRMS.Domain.Enums;

/// <summary>
/// Represents the attendance status of an employee for a given day.
/// </summary>
public enum AttendanceStatus
{
    /// <summary>Employee was present and checked in on time.</summary>
    Present = 1,

    /// <summary>Employee was absent (no check-in recorded).</summary>
    Absent = 2,

    /// <summary>Employee worked only half a day.</summary>
    HalfDay = 3,

    /// <summary>Employee checked in late (after the configured grace period).</summary>
    Late = 4,

    /// <summary>Employee checked out earlier than the standard end time.</summary>
    EarlyExit = 5,

    /// <summary>Employee was on approved leave for this day.</summary>
    OnLeave = 6
}
