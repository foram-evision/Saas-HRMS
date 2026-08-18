namespace HRMS.Application.DTOs.Attendance;

/// <summary>Request DTO for an employee checking out.</summary>
public class CheckOutDto
{
    /// <summary>Optional note at time of check-out (e.g., "Leaving early for medical appointment").</summary>
    public string? Notes { get; set; }
}
