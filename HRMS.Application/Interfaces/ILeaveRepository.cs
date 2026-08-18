using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Data access contract for leave management (LeaveTypes + LeaveRequests).
/// Implemented by LeaveRepository in the Infrastructure layer.
/// </summary>
public interface ILeaveRepository
{
    // ─── Leave Types ───────────────────────────────────────────────────────────

    Task<IEnumerable<LeaveType>> GetLeaveTypesAsync(Guid organizationId);
    Task<LeaveType?> GetLeaveTypeByIdAsync(Guid id, Guid organizationId);
    Task<LeaveType> AddLeaveTypeAsync(LeaveType leaveType);

    // ─── Leave Requests ────────────────────────────────────────────────────────

    Task<LeaveRequest?> GetLeaveRequestByIdAsync(Guid id);
    Task<IEnumerable<LeaveRequest>> GetEmployeeLeavesAsync(Guid employeeId);
    Task<IEnumerable<LeaveRequest>> GetPendingLeavesAsync(Guid organizationId);
    Task<IEnumerable<LeaveRequest>> GetApprovedLeavesForYearAsync(Guid employeeId, Guid leaveTypeId, int year);
    Task<LeaveRequest> AddLeaveRequestAsync(LeaveRequest leaveRequest);
    Task<LeaveRequest> UpdateLeaveRequestAsync(LeaveRequest leaveRequest);
    
    /// <summary>Get all leave requests in the organization, with optional status filter and pagination.</summary>
    Task<(IEnumerable<LeaveRequest> Items, int TotalCount)> GetAllLeavesAsync(Guid organizationId, LeaveStatus? status, int page, int pageSize);

    // ─── Balance Calculation ───────────────────────────────────────────────────

    /// <summary>Count total approved leave days for an employee+leaveType+year combination.</summary>
    Task<int> GetUsedDaysAsync(Guid employeeId, Guid leaveTypeId, int year);
}
