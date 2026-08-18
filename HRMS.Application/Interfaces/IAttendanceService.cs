using HRMS.Application.Common;
using HRMS.Application.DTOs.Attendance;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Application service contract for attendance management operations.
/// </summary>
public interface IAttendanceService
{
    /// <summary>
    /// Record check-in for the authenticated employee.
    /// Returns an error if already checked in today.
    /// </summary>
    Task<ApiResponse<AttendanceResponseDto>> CheckInAsync(Guid employeeId, Guid orgId, string? notes);

    /// <summary>
    /// Record check-out for the authenticated employee.
    /// Calculates TotalHours and determines final status (Late, EarlyExit, Present).
    /// Returns an error if not yet checked in today.
    /// </summary>
    Task<ApiResponse<AttendanceResponseDto>> CheckOutAsync(Guid employeeId, string? notes);

    /// <summary>Get today's attendance status for the authenticated employee.</summary>
    Task<ApiResponse<AttendanceResponseDto>> GetTodayStatusAsync(Guid employeeId);

    /// <summary>Get a monthly attendance summary with daily breakdown for an employee.</summary>
    Task<ApiResponse<AttendanceSummaryDto>> GetMonthlyReportAsync(Guid employeeId, int month, int year);

    /// <summary>Get daily attendance records for all employees in an organization. HR/Admin only.</summary>
    Task<ApiResponse<List<AttendanceResponseDto>>> GetOrgDailyReportAsync(Guid orgId, DateOnly date);

    /// <summary>Manually mark or override attendance. HR/Admin only.</summary>
    Task<ApiResponse<AttendanceResponseDto>> MarkAttendanceAsync(MarkAttendanceDto dto, Guid orgId, Guid performedByUserId);

    /// <summary>Get all attendance records for an employee in a date range. HR/Admin only.</summary>
    Task<ApiResponse<List<AttendanceResponseDto>>> GetByEmployeeAsync(Guid employeeId, Guid orgId, DateOnly fromDate, DateOnly toDate);
}
