using System.Security.Claims;
using HRMS.Application.Common;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    /// <summary>Get active leave types available in organization.</summary>
    [HttpGet("types")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<List<LeaveTypeResponseDto>>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _leaveService.GetLeaveTypesAsync(orgId.Value);
        return Ok(result);
    }

    /// <summary>Create a new leave type. Admin/HR only.</summary>
    [HttpPost("types")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> CreateLeaveType([FromBody] CreateLeaveTypeDto dto)
    {
        var orgId = GetOrganizationId();
        var userId = GetCurrentUserId();
        if (orgId == null)
            return BadRequest(ApiResponse<LeaveTypeResponseDto>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _leaveService.CreateLeaveTypeAsync(dto, orgId.Value, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Apply for leave.</summary>
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveDto dto)
    {
        var userId = GetCurrentUserId();
        var orgId = GetOrganizationId();

        if (userId == null || orgId == null)
            return Unauthorized(ApiResponse<LeaveRequestResponseDto>.Failure(AppConstants.Messages.Unauthorized));

        var result = await _leaveService.ApplyLeaveAsync(dto, userId.Value, orgId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Approve a pending leave request. Admin/HR only.</summary>
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> ApproveLeave(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _leaveService.ApproveLeaveAsync(id, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Reject a pending leave request with a reason. Admin/HR only.</summary>
    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> RejectLeave(Guid id, [FromBody] ApproveRejectLeaveDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _leaveService.RejectLeaveAsync(id, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Cancel own pending leave request.</summary>
    [HttpDelete("{id:guid}/cancel")]
    public async Task<IActionResult> CancelLeave(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _leaveService.CancelLeaveAsync(id, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get all leave requests for authenticated user.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyLeaves()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _leaveService.GetMyLeavesAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>Get pending leave requests for organization. Admin/HR only.</summary>
    [HttpGet("pending")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> GetPendingLeaves()
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<List<LeaveRequestResponseDto>>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _leaveService.GetPendingLeavesAsync(orgId.Value);
        return Ok(result);
    }

    /// <summary>Get leave balance breakdown for authenticated employee for a specific year.</summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetLeaveBalance([FromQuery] int year)
    {
        var userId = GetCurrentUserId();
        var orgId = GetOrganizationId();

        if (userId == null || orgId == null)
            return Unauthorized();

        int targetYear = year > 0 ? year : DateTime.UtcNow.Year;
        var result = await _leaveService.GetLeaveBalanceAsync(userId.Value, orgId.Value, targetYear);
        return Ok(result);
    }

    /// <summary>Get all leave requests with optional status filter and pagination. HR/Admin only.</summary>
    [HttpGet("all")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> GetAllLeaves([FromQuery] HRMS.Domain.Enums.LeaveStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<object>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _leaveService.GetAllLeavesAsync(orgId.Value, status, page, pageSize);
        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirstValue("organizationId")
                 ?? User.FindFirstValue("OrganizationId")
                 ?? User.FindFirstValue("org_id");

        if (Guid.TryParse(claim, out var orgId) && orgId != Guid.Empty)
            return orgId;

        return null;
    }
}
