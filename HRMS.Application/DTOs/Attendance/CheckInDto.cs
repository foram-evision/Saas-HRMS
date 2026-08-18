namespace HRMS.Application.DTOs.Attendance;

/// <summary>Request DTO for an employee checking in.</summary>
public class CheckInDto
{
    /// <summary>Optional note at time of check-in (e.g., "Working from home today").</summary>
    public string? Notes { get; set; }
}
