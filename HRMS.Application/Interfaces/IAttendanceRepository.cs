using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Data access contract for attendance records.
/// Implemented by AttendanceRepository in the Infrastructure layer.
/// </summary>
public interface IAttendanceRepository
{
    /// <summary>Get today's attendance record for a specific employee. Returns null if not yet checked in.</summary>
    Task<Attendance?> GetTodayAsync(Guid employeeId, DateOnly today);

    /// <summary>Get all attendance records for an employee within a specific month and year.</summary>
    Task<IEnumerable<Attendance>> GetMonthlyAsync(Guid employeeId, int month, int year);

    /// <summary>Get all attendance records for an entire organization on a specific date.</summary>
    Task<IEnumerable<Attendance>> GetOrgDailyAsync(Guid organizationId, DateOnly date);

    /// <summary>Add a new attendance record (check-in).</summary>
    Task<Attendance> AddAsync(Attendance attendance);

    /// <summary>Update an existing attendance record (check-out).</summary>
    Task<Attendance> UpdateAsync(Attendance attendance);

    /// <summary>Get all attendance records for an employee between two dates.</summary>
    Task<IEnumerable<Attendance>> GetByEmployeeAsync(Guid employeeId, DateOnly fromDate, DateOnly toDate);

    /// <summary>Insert or update an attendance record based on the date.</summary>
    Task<Attendance> UpsertAsync(Attendance attendance);
}
