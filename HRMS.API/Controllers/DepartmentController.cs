using System.Security.Claims;
using HRMS.Application.Common;
using HRMS.Application.DTOs.Department;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    /// <summary>Get all active departments in organization.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<List<DepartmentResponseDto>>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _departmentService.GetAllAsync(orgId.Value);
        return Ok(result);
    }

    /// <summary>Get department details by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<DepartmentResponseDto>.Failure(AppConstants.Messages.NoOrganization));

        var result = await _departmentService.GetByIdAsync(id, orgId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new department. Admin/HR only.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<DepartmentResponseDto>.Failure(AppConstants.Messages.NoOrganization));

        var userId = GetCurrentUserId();
        var result = await _departmentService.CreateAsync(dto, orgId.Value, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update an existing department. Admin/HR only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<DepartmentResponseDto>.Failure(AppConstants.Messages.NoOrganization));

        var userId = GetCurrentUserId();
        var result = await _departmentService.UpdateAsync(id, dto, orgId.Value, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Soft delete a department.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin}")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<bool>.Failure(AppConstants.Messages.NoOrganization));

        var userId = GetCurrentUserId();
        var result = await _departmentService.DeleteAsync(id, orgId.Value, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Assign an employee to a department.
    /// </summary>
    [HttpPost("{id}/assign-employee")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HR}")]
    public async Task<IActionResult> AssignEmployee(Guid id, [FromBody] AssignEmployeeToDepartmentDto dto)
    {
        var orgId = GetOrganizationId();
        if (orgId == null)
            return BadRequest(ApiResponse<bool>.Failure(AppConstants.Messages.NoOrganization));

        var userId = GetCurrentUserId();
        var result = await _departmentService.AssignEmployeeAsync(id, dto.EmployeeId, orgId.Value, userId);
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