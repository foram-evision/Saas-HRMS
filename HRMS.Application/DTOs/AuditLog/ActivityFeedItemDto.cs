namespace HRMS.Application.DTOs.AuditLog;

/// <summary>
/// Compact, user-friendly activity feed entry.
/// Returns a human-readable description instead of raw action codes.
/// Used by the activity feed endpoint consumed by dashboards and notification panels.
/// </summary>
public class ActivityFeedItemDto
{
    public Guid Id { get; set; }

    /// <summary>Human-readable description of the activity (e.g., "John Doe checked in at 09:15 AM").</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Raw action code for programmatic use (e.g., "CheckIn").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Entity type affected (e.g., "Attendance", "Employee").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Name of the user who performed the action.</summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the action.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Time ago string (e.g., "2 hours ago", "yesterday").</summary>
    public string TimeAgo { get; set; } = string.Empty;
}
