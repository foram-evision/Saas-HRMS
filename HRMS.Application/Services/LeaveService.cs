using HRMS.Application.Common;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

/// <summary>
/// Business logic for leave management — apply, approve, reject, cancel, balance.
/// </summary>
public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository    _leaveRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAuditLogService    _auditLogService;

    public LeaveService(
        ILeaveRepository    leaveRepository,
        IEmployeeRepository employeeRepository,
        IAuditLogService    auditLogService)
    {
        _leaveRepository    = leaveRepository;
        _employeeRepository = employeeRepository;
        _auditLogService    = auditLogService;
    }

    // ─── Leave Types ───────────────────────────────────────────────────────────

    public async Task<ApiResponse<List<LeaveTypeResponseDto>>> GetLeaveTypesAsync(Guid orgId)
    {
        var types = await _leaveRepository.GetLeaveTypesAsync(orgId);
        var dtos  = types.Select(MapLeaveTypeToDto).ToList();
        return ApiResponse<List<LeaveTypeResponseDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<LeaveTypeResponseDto>> CreateLeaveTypeAsync(CreateLeaveTypeDto dto, Guid orgId, Guid? performedByUserId = null)
    {
        var leaveType = new LeaveType
        {
            Name             = dto.Name.Trim(),
            Description      = dto.Description?.Trim(),
            TotalDaysAllowed = dto.TotalDaysAllowed,
            OrganizationId   = orgId,
            IsActive         = true
        };

        var saved = await _leaveRepository.AddLeaveTypeAsync(leaveType);

        await _auditLogService.LogAsync(
            performedByUserId ?? Guid.Empty, orgId,
            AppConstants.AuditEvents.LeaveTypeCreated, "LeaveType", saved.Id,
            newValues: $"{{\"Name\":\"{saved.Name}\",\"Days\":{saved.TotalDaysAllowed}}}");

        return ApiResponse<LeaveTypeResponseDto>.SuccessResult(
            MapLeaveTypeToDto(saved), "Leave type created successfully.");
    }

    // ─── Apply Leave ───────────────────────────────────────────────────────────

    public async Task<ApiResponse<LeaveRequestResponseDto>> ApplyLeaveAsync(
        ApplyLeaveDto dto, Guid userIdOrEmployeeId, Guid orgId)
    {
        var employee = await ResolveEmployeeAsync(userIdOrEmployeeId, orgId);
        if (employee == null)
            return ApiResponse<LeaveRequestResponseDto>.Failure("Employee profile not found.");

        var employeeId = employee.Id;

        // Validate date range
        if (dto.EndDate < dto.StartDate)
            return ApiResponse<LeaveRequestResponseDto>.Failure("End date cannot be before start date.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dto.StartDate < today)
            return ApiResponse<LeaveRequestResponseDto>.Failure("Cannot apply leave for past dates.");

        // Validate leave type belongs to org
        var leaveType = await _leaveRepository.GetLeaveTypeByIdAsync(dto.LeaveTypeId, orgId);
        if (leaveType is null || !leaveType.IsActive)
            return ApiResponse<LeaveRequestResponseDto>.Failure("Invalid or inactive leave type.");

        int totalDays = dto.EndDate.DayNumber - dto.StartDate.DayNumber + 1;

        // Check leave balance
        int usedDays = await _leaveRepository.GetUsedDaysAsync(employeeId, dto.LeaveTypeId, dto.StartDate.Year);
        int remaining = leaveType.TotalDaysAllowed - usedDays;
        if (totalDays > remaining)
            return ApiResponse<LeaveRequestResponseDto>.Failure(
                $"Insufficient leave balance. Remaining: {remaining} days, Requested: {totalDays} days.");

        var request = new LeaveRequest
        {
            EmployeeId     = employeeId,
            OrganizationId = orgId,
            LeaveTypeId    = dto.LeaveTypeId,
            StartDate      = dto.StartDate,
            EndDate        = dto.EndDate,
            TotalDays      = totalDays,
            Reason         = dto.Reason.Trim(),
            Status         = LeaveStatus.Pending
        };

        var saved = await _leaveRepository.AddLeaveRequestAsync(request);

        await _auditLogService.LogAsync(
            userIdOrEmployeeId, orgId,
            AppConstants.AuditEvents.LeaveApplied, "LeaveRequest", saved.Id,
            newValues: $"{{\"LeaveType\":\"{leaveType.Name}\",\"Days\":{totalDays}}}");

        return ApiResponse<LeaveRequestResponseDto>.SuccessResult(
            MapLeaveRequestToDto(saved, leaveType.Name, null),
            "Leave request submitted successfully.");
    }

    // ─── Approve Leave ────────────────────────────────────────────────────────

    public async Task<ApiResponse<LeaveRequestResponseDto>> ApproveLeaveAsync(Guid leaveId, Guid approvedByUserId)
    {
        var request = await _leaveRepository.GetLeaveRequestByIdAsync(leaveId);
        if (request is null)
            return ApiResponse<LeaveRequestResponseDto>.Failure(AppConstants.Messages.LeaveRequestNotFound);

        if (request.Status != LeaveStatus.Pending)
            return ApiResponse<LeaveRequestResponseDto>.Failure(
                $"Cannot approve a leave that is already {request.Status}.");

        request.Status          = LeaveStatus.Approved;
        request.ApprovedByUserId = approvedByUserId;
        request.ActionedAt      = DateTime.UtcNow;
        request.UpdatedAt       = DateTime.UtcNow;

        var updated = await _leaveRepository.UpdateLeaveRequestAsync(request);

        await _auditLogService.LogAsync(
            approvedByUserId, request.OrganizationId,
            AppConstants.AuditEvents.LeaveApproved, "LeaveRequest", leaveId);

        return ApiResponse<LeaveRequestResponseDto>.SuccessResult(
            MapLeaveRequestToDto(updated, updated.LeaveType?.Name ?? string.Empty, null),
            "Leave request approved.");
    }

    // ─── Reject Leave ─────────────────────────────────────────────────────────

    public async Task<ApiResponse<LeaveRequestResponseDto>> RejectLeaveAsync(
        Guid leaveId, Guid rejectedByUserId, ApproveRejectLeaveDto dto)
    {
        var request = await _leaveRepository.GetLeaveRequestByIdAsync(leaveId);
        if (request is null)
            return ApiResponse<LeaveRequestResponseDto>.Failure(AppConstants.Messages.LeaveRequestNotFound);

        if (request.Status != LeaveStatus.Pending)
            return ApiResponse<LeaveRequestResponseDto>.Failure(
                $"Cannot reject a leave that is already {request.Status}.");

        request.Status           = LeaveStatus.Rejected;
        request.ApprovedByUserId  = rejectedByUserId;
        request.RejectionReason  = dto.RejectionReason;
        request.ActionedAt       = DateTime.UtcNow;
        request.UpdatedAt        = DateTime.UtcNow;

        var updated = await _leaveRepository.UpdateLeaveRequestAsync(request);

        await _auditLogService.LogAsync(
            rejectedByUserId, request.OrganizationId,
            AppConstants.AuditEvents.LeaveRejected, "LeaveRequest", leaveId,
            newValues: $"{{\"Reason\":\"{dto.RejectionReason}\"}}");

        return ApiResponse<LeaveRequestResponseDto>.SuccessResult(
            MapLeaveRequestToDto(updated, updated.LeaveType?.Name ?? string.Empty, null),
            "Leave request rejected.");
    }

    // ─── Cancel Leave ─────────────────────────────────────────────────────────

    public async Task<ApiResponse<bool>> CancelLeaveAsync(Guid leaveId, Guid employeeId)
    {
        var request = await _leaveRepository.GetLeaveRequestByIdAsync(leaveId);
        if (request is null)
            return ApiResponse<bool>.Failure(AppConstants.Messages.LeaveRequestNotFound);

        if (request.EmployeeId != employeeId)
            return ApiResponse<bool>.Failure(AppConstants.Messages.Unauthorized);

        if (request.Status != LeaveStatus.Pending)
            return ApiResponse<bool>.Failure("Only pending leave requests can be cancelled.");

        request.Status    = LeaveStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await _leaveRepository.UpdateLeaveRequestAsync(request);

        await _auditLogService.LogAsync(
            employeeId, request.OrganizationId,
            AppConstants.AuditEvents.LeaveCancelled, "LeaveRequest", leaveId);

        return ApiResponse<bool>.SuccessResult(true, "Leave request cancelled.");
    }

    // ─── My Leaves ────────────────────────────────────────────────────────────

    public async Task<ApiResponse<List<LeaveRequestResponseDto>>> GetMyLeavesAsync(Guid userIdOrEmployeeId)
    {
        var employee = await _employeeRepository.GetByUserIdAsync(userIdOrEmployeeId)
                    ?? await _employeeRepository.GetByIdAsync(userIdOrEmployeeId);

        var employeeId = employee?.Id ?? userIdOrEmployeeId;
        var leaves = await _leaveRepository.GetEmployeeLeavesAsync(employeeId);
        var dtos   = leaves.Select(l =>
            MapLeaveRequestToDto(l, l.LeaveType?.Name ?? string.Empty, l.ApprovedByUser?.FullName)).ToList();
        return ApiResponse<List<LeaveRequestResponseDto>>.SuccessResult(dtos);
    }

    // ─── Pending Leaves (HR) ──────────────────────────────────────────────────

    public async Task<ApiResponse<List<LeaveRequestResponseDto>>> GetPendingLeavesAsync(Guid orgId)
    {
        var leaves = await _leaveRepository.GetPendingLeavesAsync(orgId);
        var dtos   = leaves.Select(l =>
            MapLeaveRequestToDto(l, l.LeaveType?.Name ?? string.Empty, null)).ToList();
        return ApiResponse<List<LeaveRequestResponseDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<object>> GetAllLeavesAsync(Guid orgId, HRMS.Domain.Enums.LeaveStatus? status, int page, int pageSize)
    {
        var (items, totalCount) = await _leaveRepository.GetAllLeavesAsync(orgId, status, page, pageSize);
        
        var dtos = items.Select(l => MapLeaveRequestToDto(l, l.LeaveType?.Name ?? string.Empty, l.ApprovedByUser?.FullName)).ToList();
        
        var result = new
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return ApiResponse<object>.SuccessResult(result);
    }

    // ─── Leave Balance ────────────────────────────────────────────────────────

    public async Task<ApiResponse<LeaveBalanceDto>> GetLeaveBalanceAsync(Guid userIdOrEmployeeId, Guid orgId, int year)
    {
        var employee = await ResolveEmployeeAsync(userIdOrEmployeeId, orgId);
        var employeeId = employee?.Id ?? userIdOrEmployeeId;

        var leaveTypes = (await _leaveRepository.GetLeaveTypesAsync(orgId))
            .Where(lt => lt.IsActive)
            .ToList();

        var balances = new List<LeaveTypeBalanceDto>();

        foreach (var lt in leaveTypes)
        {
            int used      = await _leaveRepository.GetUsedDaysAsync(employeeId, lt.Id, year);
            int remaining = Math.Max(0, lt.TotalDaysAllowed - used);

            balances.Add(new LeaveTypeBalanceDto
            {
                LeaveTypeId   = lt.Id,
                LeaveTypeName = lt.Name,
                TotalAllowed  = lt.TotalDaysAllowed,
                UsedDays      = used,
                RemainingDays = remaining
            });
        }

        var dto = new LeaveBalanceDto
        {
            EmployeeId   = employeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : string.Empty,
            Year         = year,
            Balances     = balances
        };

        return ApiResponse<LeaveBalanceDto>.SuccessResult(dto);
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

    // ─── Mappers ──────────────────────────────────────────────────────────────

    private static LeaveTypeResponseDto MapLeaveTypeToDto(LeaveType lt) => new()
    {
        Id               = lt.Id,
        Name             = lt.Name,
        Description      = lt.Description,
        TotalDaysAllowed = lt.TotalDaysAllowed,
        IsActive         = lt.IsActive
    };

    private static LeaveRequestResponseDto MapLeaveRequestToDto(
        LeaveRequest r, string leaveTypeName, string? approvedByName) => new()
    {
        Id              = r.Id,
        EmployeeId      = r.EmployeeId,
        EmployeeName    = r.Employee?.User?.FullName ?? string.Empty,
        LeaveTypeId     = r.LeaveTypeId,
        LeaveTypeName   = leaveTypeName,
        StartDate       = r.StartDate,
        EndDate         = r.EndDate,
        TotalDays       = r.TotalDays,
        Reason          = r.Reason,
        Status          = r.Status,
        ApprovedByName  = approvedByName,
        RejectionReason = r.RejectionReason,
        ActionedAt      = r.ActionedAt,
        CreatedAt       = r.CreatedAt
    };
}
