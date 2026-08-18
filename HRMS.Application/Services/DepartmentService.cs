using HRMS.Application.Common;
using HRMS.Application.DTOs.Department;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HRMS.Application.Services;

/// <summary>
/// Business logic for department management.
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        IAuditLogService auditLogService,
        UserManager<ApplicationUser> userManager)
    {
        _departmentRepository = departmentRepository;
        _auditLogService = auditLogService;
        _userManager = userManager;
    }

    public async Task<ApiResponse<List<DepartmentResponseDto>>> GetAllAsync(Guid orgId)
    {
        var departments = await _departmentRepository.GetAllAsync(orgId);
        var dtos = new List<DepartmentResponseDto>();

        foreach (var dept in departments)
        {
            int empCount = await _departmentRepository.GetEmployeeCountAsync(dept.Id);
            dtos.Add(MapToDto(dept, empCount));
        }

        return ApiResponse<List<DepartmentResponseDto>>.SuccessResult(dtos);
    }

    public async Task<ApiResponse<DepartmentResponseDto>> GetByIdAsync(Guid id, Guid orgId)
    {
        var dept = await _departmentRepository.GetByIdAsync(id, orgId);
        if (dept is null)
            return ApiResponse<DepartmentResponseDto>.Failure(AppConstants.Messages.DepartmentNotFound);

        int empCount = await _departmentRepository.GetEmployeeCountAsync(dept.Id);
        return ApiResponse<DepartmentResponseDto>.SuccessResult(MapToDto(dept, empCount));
    }

    public async Task<ApiResponse<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, Guid orgId, Guid? performedByUserId = null)
    {
        if (orgId == Guid.Empty)
            return ApiResponse<DepartmentResponseDto>.Failure(AppConstants.Messages.NoOrganization);

        if (await _departmentRepository.ExistsByNameAsync(dto.Name.Trim(), orgId))
            return ApiResponse<DepartmentResponseDto>.Failure("A department with this name already exists.");

        Guid? managerId = null;
        if (dto.ManagerId.HasValue && dto.ManagerId.Value != Guid.Empty)
        {
            var manager = await _userManager.FindByIdAsync(dto.ManagerId.Value.ToString());
            if (manager != null)
            {
                managerId = manager.Id;
            }
        }

        var department = new Department
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            OrganizationId = orgId,
            ManagerId = managerId,
            IsActive = true
        };

        var saved = await _departmentRepository.AddAsync(department);

        await _auditLogService.LogAsync(
            performedByUserId ?? Guid.Empty, orgId,
            AppConstants.AuditEvents.DepartmentCreated, "Department", saved.Id,
            newValues: $"{{\"Name\":\"{saved.Name}\"}}");

        return ApiResponse<DepartmentResponseDto>.SuccessResult(MapToDto(saved, 0), "Department created successfully.");
    }

    public async Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(Guid id, UpdateDepartmentDto dto, Guid orgId, Guid? performedByUserId = null)
    {
        var dept = await _departmentRepository.GetByIdAsync(id, orgId);
        if (dept is null)
            return ApiResponse<DepartmentResponseDto>.Failure(AppConstants.Messages.DepartmentNotFound);

        if (await _departmentRepository.ExistsByNameAsync(dto.Name.Trim(), orgId, id))
            return ApiResponse<DepartmentResponseDto>.Failure("A department with this name already exists.");

        dept.Name = dto.Name.Trim();
        dept.Description = dto.Description?.Trim();

        Guid? managerId = null;
        if (dto.ManagerId.HasValue && dto.ManagerId.Value != Guid.Empty)
        {
            var manager = await _userManager.FindByIdAsync(dto.ManagerId.Value.ToString());
            if (manager != null)
            {
                managerId = manager.Id;
            }
        }

        dept.ManagerId = managerId;
        dept.IsActive = dto.IsActive;
        dept.UpdatedAt = DateTime.UtcNow;

        var updated = await _departmentRepository.UpdateAsync(dept);
        int empCount = await _departmentRepository.GetEmployeeCountAsync(updated.Id);

        await _auditLogService.LogAsync(
            performedByUserId ?? Guid.Empty, orgId,
            AppConstants.AuditEvents.DepartmentUpdated, "Department", id);

        return ApiResponse<DepartmentResponseDto>.SuccessResult(MapToDto(updated, empCount), "Department updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid orgId, Guid? performedByUserId = null)
    {
        var result = await _departmentRepository.SoftDeleteAsync(id, orgId);
        if (!result)
            return ApiResponse<bool>.Failure(AppConstants.Messages.DepartmentNotFound);

        if (performedByUserId.HasValue)
        {
            await _auditLogService.LogAsync(
                performedByUserId.Value, orgId,
                AppConstants.AuditEvents.DepartmentDeleted, "Department", id);
        }

        return ApiResponse<bool>.SuccessResult(true, "Department deleted successfully.");
    }

    public async Task<ApiResponse<bool>> AssignEmployeeAsync(Guid departmentId, Guid employeeId, Guid orgId, Guid? performedByUserId = null)
    {
        var result = await _departmentRepository.AssignEmployeeAsync(employeeId, departmentId, orgId);
        
        if (!result)
            return ApiResponse<bool>.Failure("Department or Employee not found, or employee doesn't belong to this organization.");

        if (performedByUserId.HasValue)
        {
            await _auditLogService.LogAsync(
                performedByUserId.Value, orgId,
                AppConstants.AuditEvents.EmployeeAssignedToDepartment, "Employee", employeeId,
                newValues: $"{{\"DepartmentId\":\"{departmentId}\"}}");
        }

        return ApiResponse<bool>.SuccessResult(true, "Employee successfully assigned to department.");
    }

    private static DepartmentResponseDto MapToDto(Department dept, int employeeCount) => new()
    {
        Id = dept.Id,
        Name = dept.Name,
        Description = dept.Description,
        OrganizationId = dept.OrganizationId,
        ManagerId = dept.ManagerId,
        ManagerName = dept.Manager?.FullName,
        IsActive = dept.IsActive,
        EmployeeCount = employeeCount,
        CreatedAt = dept.CreatedAt
    };
}
