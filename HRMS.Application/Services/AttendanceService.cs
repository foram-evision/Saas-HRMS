using HRMS.Application.Common;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

/// <summary>
/// Business logic for attendance management.
/// Standard office hours: 9:00 AM start, entries after 9:30 AM are marked Late.
/// Standard work day: 8 hours. Checkout before 5:00 PM is marked EarlyExit.
/// </summary>
public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEmployeeRepository   _employeeRepository;
    private readonly IAuditLogService      _auditLogService;

    // ─── Policy Constants ──────────────────────────────────────────────────────
    private static readonly TimeSpan StandardStartTime = new(9, 0, 0);   // 9:00 AM
    private static readonly TimeSpan GracePeriod       = new(0, 30, 0);  // 30 min grace
    private static readonly TimeSpan StandardEndTime   = new(17, 0, 0);  // 5:00 PM

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IEmployeeRepository   employeeRepository,
        IAuditLogService      auditLogService)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository   = employeeRepository;
        _auditLogService      = auditLogService;
    }

    // ─── Check In ─────────────────────────────────────────────────────────────

    public async Task<ApiResponse<AttendanceResponseDto>> CheckInAsync(Guid userIdOrEmployeeId, Guid orgId, string? notes)
    {
        var employee = await ResolveEmployeeAsync(userIdOrEmployeeId, orgId);
        if (employee == null)
            return ApiResponse<AttendanceResponseDto>.Failure("Employee profile not found and could not be created.");

        var employeeId = employee.Id;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Prevent duplicate check-in for the same day
        var existing = await _attendanceRepository.GetTodayAsync(employeeId, today);
        if (existing is not null)
            return ApiResponse<AttendanceResponseDto>.Failure("You have already checked in today.");

        var checkInTime = DateTime.UtcNow;
        var localTime   = checkInTime.TimeOfDay;

        // Determine status based on check-in time vs policy
        var status = localTime > (StandardStartTime + GracePeriod)
            ? AttendanceStatus.Late
            : AttendanceStatus.Present;

        var attendance = new Attendance
        {
            EmployeeId     = employeeId,
            OrganizationId = orgId,
            Date           = today,
            CheckIn        = checkInTime,
            Status         = status,
            Notes          = notes
        };

        var saved = await _attendanceRepository.AddAsync(attendance);

        await _auditLogService.LogAsync(
            userIdOrEmployeeId, orgId,
            AppConstants.AuditEvents.CheckIn, "Attendance", saved.Id);

        return ApiResponse<AttendanceResponseDto>.SuccessResult(
            MapToDto(saved, $"{employee.FirstName} {employee.LastName}".Trim()),
            $"Check-in recorded at {checkInTime:HH:mm} UTC. Status: {status}.");
    }

    // ─── Check Out ────────────────────────────────────────────────────────────

    public async Task<ApiResponse<AttendanceResponseDto>> CheckOutAsync(Guid userIdOrEmployeeId, string? notes)
    {
        var employee = await _employeeRepository.GetByUserIdAsync(userIdOrEmployeeId)
                    ?? await _employeeRepository.GetByIdAsync(userIdOrEmployeeId);

        var employeeId = employee?.Id ?? userIdOrEmployeeId;
        var today    = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = await _attendanceRepository.GetTodayAsync(employeeId, today);

        if (existing is null)
            return ApiResponse<AttendanceResponseDto>.Failure("No check-in found for today. Please check in first.");

        if (existing.CheckOut is not null)
            return ApiResponse<AttendanceResponseDto>.Failure("You have already checked out today.");

        var checkOutTime = DateTime.UtcNow;
        existing.CheckOut   = checkOutTime;
        existing.TotalHours = (decimal)(checkOutTime - existing.CheckIn!.Value).TotalHours;
        existing.UpdatedAt  = DateTime.UtcNow;

        // Append notes
        if (!string.IsNullOrWhiteSpace(notes))
            existing.Notes = string.IsNullOrWhiteSpace(existing.Notes)
                ? notes
                : $"{existing.Notes} | {notes}";

        // Determine if early exit
        if (checkOutTime.TimeOfDay < StandardEndTime && existing.Status != AttendanceStatus.Late)
            existing.Status = AttendanceStatus.EarlyExit;

        // Half day: less than 4 hours worked
        if (existing.TotalHours < 4m)
            existing.Status = AttendanceStatus.HalfDay;

        var updated = await _attendanceRepository.UpdateAsync(existing);

        await _auditLogService.LogAsync(
            userIdOrEmployeeId, existing.OrganizationId,
            AppConstants.AuditEvents.CheckOut, "Attendance", existing.Id);

        return ApiResponse<AttendanceResponseDto>.SuccessResult(
            MapToDto(updated, string.Empty),
            $"Check-out recorded. Total hours: {existing.TotalHours:F1}h.");
    }

    // ─── Today Status ─────────────────────────────────────────────────────────

    public async Task<ApiResponse<AttendanceResponseDto>> GetTodayStatusAsync(Guid userIdOrEmployeeId)
    {
        var employee = await _employeeRepository.GetByUserIdAsync(userIdOrEmployeeId)
                    ?? await _employeeRepository.GetByIdAsync(userIdOrEmployeeId);

        var employeeId = employee?.Id ?? userIdOrEmployeeId;
        var today    = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = await _attendanceRepository.GetTodayAsync(employeeId, today);

        if (existing is null)
            return ApiResponse<AttendanceResponseDto>.Failure("No attendance record found for today.");

        return ApiResponse<AttendanceResponseDto>.SuccessResult(MapToDto(existing, string.Empty));
    }

    // ─── Monthly Report ───────────────────────────────────────────────────────

    public async Task<ApiResponse<AttendanceSummaryDto>> GetMonthlyReportAsync(Guid userIdOrEmployeeId, int month, int year)
    {
        if (month < 1 || month > 12)
            return ApiResponse<AttendanceSummaryDto>.Failure("Month must be between 1 and 12.");

        var employee = await _employeeRepository.GetByUserIdAsync(userIdOrEmployeeId)
                    ?? await _employeeRepository.GetByIdAsync(userIdOrEmployeeId);

        var employeeId = employee?.Id ?? userIdOrEmployeeId;
        var records = (await _attendanceRepository.GetMonthlyAsync(employeeId, month, year)).ToList();

        var summary = new AttendanceSummaryDto
        {
            EmployeeId      = employeeId,
            EmployeeName    = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : string.Empty,
            Month           = month,
            Year            = year,
            TotalWorkingDays = records.Count,
            PresentDays     = records.Count(r => r.Status == AttendanceStatus.Present),
            AbsentDays      = records.Count(r => r.Status == AttendanceStatus.Absent),
            LateDays        = records.Count(r => r.Status == AttendanceStatus.Late),
            HalfDays        = records.Count(r => r.Status == AttendanceStatus.HalfDay),
            LeaveDays       = records.Count(r => r.Status == AttendanceStatus.OnLeave),
            TotalHoursWorked = records.Sum(r => r.TotalHours ?? 0m),
            DailyRecords    = records.Select(r => MapToDto(r, string.Empty)).ToList()
        };

        return ApiResponse<AttendanceSummaryDto>.SuccessResult(summary);
    }

    // ─── Org Daily Report ─────────────────────────────────────────────────────

    public async Task<ApiResponse<List<AttendanceResponseDto>>> GetOrgDailyReportAsync(Guid orgId, DateOnly date)
    {
        var records = await _attendanceRepository.GetOrgDailyAsync(orgId, date);
        var dtos = records.Select(r => MapToDto(r, string.Empty)).ToList();
        return ApiResponse<List<AttendanceResponseDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<AttendanceResponseDto>> MarkAttendanceAsync(MarkAttendanceDto dto, Guid orgId, Guid performedByUserId)
    {
        var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
        if (employee == null || employee.OrganizationId != orgId)
            return ApiResponse<AttendanceResponseDto>.Failure(AppConstants.Messages.EmployeeNotFound);

        var totalHours = 0m;
        if (dto.CheckIn.HasValue && dto.CheckOut.HasValue)
        {
            totalHours = (decimal)(dto.CheckOut.Value - dto.CheckIn.Value).TotalHours;
        }

        var attendance = new Attendance
        {
            EmployeeId = dto.EmployeeId,
            Date = dto.Date,
            CheckIn = dto.CheckIn,
            CheckOut = dto.CheckOut,
            Status = dto.Status,
            TotalHours = totalHours,
            Notes = dto.Notes ?? "Marked by HR"
        };

        var result = await _attendanceRepository.UpsertAsync(attendance);

        await _auditLogService.LogAsync(
            performedByUserId, orgId,
            AppConstants.AuditEvents.AttendanceMarked, "Attendance", result.Id,
            newValues: $"{{\"Date\":\"{dto.Date}\",\"Status\":\"{dto.Status}\"}}");

        return ApiResponse<AttendanceResponseDto>.SuccessResult(MapToDto(result, string.Empty), "Attendance marked successfully.");
    }

    public async Task<ApiResponse<List<AttendanceResponseDto>>> GetByEmployeeAsync(Guid employeeId, Guid orgId, DateOnly fromDate, DateOnly toDate)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null || employee.OrganizationId != orgId)
            return ApiResponse<List<AttendanceResponseDto>>.Failure(AppConstants.Messages.EmployeeNotFound);

        var records = await _attendanceRepository.GetByEmployeeAsync(employeeId, fromDate, toDate);
        var dtos = records.Select(r => MapToDto(r, string.Empty)).ToList();
        return ApiResponse<List<AttendanceResponseDto>>.SuccessResult(dtos);
    }

    // ─── Helper: Resolve Employee ─────────────────────────────────────────────

    private async Task<Employee?> ResolveEmployeeAsync(Guid id, Guid orgId)
    {
        // 1. Check if ID matches an Employee.UserId
        var emp = await _employeeRepository.GetByUserIdAsync(id);
        if (emp != null) return emp;

        // 2. Check if ID matches an Employee.Id directly
        emp = await _employeeRepository.GetByIdAsync(id);
        if (emp != null) return emp;

        // 3. Auto-create Employee profile for the User if it doesn't exist yet
        var newEmp = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = id,
            FirstName = "User",
            LastName = "Member",
            Address = "Office",
            OrganizationId = orgId,
            CreatedBy = id
        };

        return await _employeeRepository.CreateAsync(newEmp);
    }

    // ─── Mapper ───────────────────────────────────────────────────────────────

    private static AttendanceResponseDto MapToDto(Attendance a, string employeeName) => new()
    {
        Id           = a.Id,
        EmployeeId   = a.EmployeeId,
        EmployeeName = employeeName,
        Date         = a.Date,
        CheckIn      = a.CheckIn,
        CheckOut     = a.CheckOut,
        TotalHours   = a.TotalHours,
        Status       = a.Status,
        Notes        = a.Notes
    };
}
