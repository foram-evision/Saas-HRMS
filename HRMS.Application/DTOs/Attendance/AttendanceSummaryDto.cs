namespace HRMS.Application.DTOs.Attendance;

/// <summary>
/// Monthly attendance summary for a single employee.
/// Returned by the monthly report endpoint.
/// </summary>
public class AttendanceSummaryDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int HalfDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public List<AttendanceResponseDto> DailyRecords { get; set; } = new();
}
