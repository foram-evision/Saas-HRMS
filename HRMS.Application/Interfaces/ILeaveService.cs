using HRMS.Application.Common;
using HRMS.Application.DTOs.Leave;
using HRMS.Domain.Enums;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Application service contract for all leave management operations.
/// </summary>
public interface ILeaveService
{
    // ─── Leave Types ───────────────────────────────────────────────────────────

    Task<ApiResponse<List<LeaveTypeResponseDto>>> GetLeaveTypesAsync(Guid orgId);
    Task<ApiResponse<LeaveTypeResponseDto>> CreateLeaveTypeAsync(CreateLeaveTypeDto dto, Guid orgId, Guid? performedByUserId = null);

    // ─── Leave Requests ────────────────────────────────────────────────────────

    /// <summary>Employee applies for leave. Validates dates, balance, and overlaps.</summary>
    Task<ApiResponse<LeaveRequestResponseDto>> ApplyLeaveAsync(ApplyLeaveDto dto, Guid employeeId, Guid orgId);

    /// <summary>HR/Admin approves a pending leave request.</summary>
    Task<ApiResponse<LeaveRequestResponseDto>> ApproveLeaveAsync(Guid leaveId, Guid approvedByUserId);

    /// <summary>HR/Admin rejects a pending leave request with a reason.</summary>
    Task<ApiResponse<LeaveRequestResponseDto>> RejectLeaveAsync(Guid leaveId, Guid rejectedByUserId, ApproveRejectLeaveDto dto);

    /// <summary>Employee cancels their own pending leave request.</summary>
    Task<ApiResponse<bool>> CancelLeaveAsync(Guid leaveId, Guid employeeId);

    /// <summary>Get all leave requests for the authenticated employee.</summary>
    Task<ApiResponse<List<LeaveRequestResponseDto>>> GetMyLeavesAsync(Guid employeeId);

    /// <summary>Get all pending leave requests for an organization. HR/Admin only.</summary>
    Task<ApiResponse<List<LeaveRequestResponseDto>>> GetPendingLeavesAsync(Guid orgId);

    /// <summary>Get paginated leave requests for an organization with optional status filter. HR/Admin only.</summary>
    Task<ApiResponse<object>> GetAllLeavesAsync(Guid orgId, LeaveStatus? status, int page, int pageSize);

    // ─── Leave Balance ─────────────────────────────────────────────────────────

    Task<ApiResponse<LeaveBalanceDto>> GetLeaveBalanceAsync(Guid employeeId, Guid orgId, int year);
}
