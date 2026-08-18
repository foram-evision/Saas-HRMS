using System.Security.Claims;
using HRMS.Application.Common;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    /// <summary>Generate salary for an employee. Admin/HR only.</summary>
    [HttpPost("generate")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> GenerateSalary([FromBody] GenerateSalaryDto dto)
    {
        var userId = GetCurrentUserId();
        var orgId = GetOrganizationId();

        if (userId == null || orgId == null)
            return Unauthorized();

        var result = await _payrollService.GenerateSalaryAsync(dto, orgId.Value, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get detailed payslip for a salary record.</summary>
    [HttpGet("{id:guid}/payslip")]
    public async Task<IActionResult> GetPayslip(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        bool isHrOrAdmin = User.IsInRole(AppConstants.Roles.Admin) || User.IsInRole(AppConstants.Roles.HR);
        var result = await _payrollService.GetPayslipAsync(id, userId.Value, isHrOrAdmin);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get personal payroll history for current employee.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyPayrollHistory()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _payrollService.GetMyPayrollHistoryAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>Get organization payroll run for a month and year. Admin/HR only.</summary>
    [HttpGet("org")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> GetOrgPayroll([FromQuery] int month, [FromQuery] int year)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<List<SalaryResponseDto>>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _payrollService.GetOrgPayrollAsync(orgId.Value, month, year);
        return Ok(result);
    }

    /// <summary>Mark a generated salary as Paid. Admin/HR only.</summary>
    [HttpPut("{id:guid}/mark-paid")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> MarkAsPaid(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _payrollService.MarkAsPaidAsync(id, userId.Value);
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
