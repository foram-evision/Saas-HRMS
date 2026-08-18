using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.Attendance;

/// <summary>
/// Response DTO for a single attendance record.
/// Returned by check-in, check-out, and individual record queries.
/// </summary>
public class AttendanceResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public decimal? TotalHours { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
}
