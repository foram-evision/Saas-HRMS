using System.Security.Claims;
using HRMS.Application.Common;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    /// <summary>Employee checks in for today.</summary>
    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
    {
        var employeeId = GetCurrentUserId();
        var orgId = GetOrganizationId();

        if (employeeId == null || orgId == null)
            return Unauthorized(ApiResponse<AttendanceResponseDto>.Failure(AppConstants.Messages.Unauthorized));

        var result = await _attendanceService.CheckInAsync(employeeId.Value, orgId.Value, dto.Notes);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Employee checks out for today.</summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto)
    {
        var employeeId = GetCurrentUserId();
        if (employeeId == null)
            return Unauthorized(ApiResponse<AttendanceResponseDto>.Failure(AppConstants.Messages.Unauthorized));

        var result = await _attendanceService.CheckOutAsync(employeeId.Value, dto.Notes);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get today's attendance status for current employee.</summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayStatus()
    {
        var employeeId = GetCurrentUserId();
        if (employeeId == null)
            return Unauthorized(ApiResponse<AttendanceResponseDto>.Failure(AppConstants.Messages.Unauthorized));

        var result = await _attendanceService.GetTodayStatusAsync(employeeId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get monthly attendance summary report for current employee.</summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int month, [FromQuery] int year)
    {
        var employeeId = GetCurrentUserId();
        if (employeeId == null)
            return Unauthorized(ApiResponse<AttendanceSummaryDto>.Failure(AppConstants.Messages.Unauthorized));

        var result = await _attendanceService.GetMonthlyReportAsync(employeeId.Value, month, year);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get daily attendance report for all employees in organization. HR/Admin only.</summary>
    [HttpGet("org-daily")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> GetOrgDailyReport([FromQuery] DateOnly date)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<List<AttendanceResponseDto>>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _attendanceService.GetOrgDailyReportAsync(orgId.Value, date);
        return Ok(result);
    }

    /// <summary>HR/Admin manually marks or overrides attendance.</summary>
    [HttpPost("mark")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDto dto)
    {
        var orgId = GetOrganizationId();
        var userId = GetCurrentUserId();

        if (orgId == null || userId == null)
            return BadRequest(ApiResponse<AttendanceResponseDto>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _attendanceService.MarkAttendanceAsync(dto, orgId.Value, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get attendance history for a specific employee in a date range. HR/Admin only.</summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> GetEmployeeAttendance(Guid employeeId, [FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<List<AttendanceResponseDto>>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _attendanceService.GetByEmployeeAsync(employeeId, orgId.Value, from, to);
        return result.Success ? Ok(result) : BadRequest(result);
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
