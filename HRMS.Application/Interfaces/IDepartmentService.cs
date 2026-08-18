using HRMS.Application.Common;
using HRMS.Application.DTOs.Department;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Application service contract for department management operations.
/// </summary>
public interface IDepartmentService
{
    Task<ApiResponse<List<DepartmentResponseDto>>> GetAllAsync(Guid orgId);
    Task<ApiResponse<DepartmentResponseDto>> GetByIdAsync(Guid id, Guid orgId);
    Task<ApiResponse<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, Guid orgId, Guid? performedByUserId = null);
    Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(Guid id, UpdateDepartmentDto dto, Guid orgId, Guid? performedByUserId = null);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid orgId, Guid? performedByUserId = null);
    
    /// <summary>Assigns an employee to a department.</summary>
    Task<ApiResponse<bool>> AssignEmployeeAsync(Guid departmentId, Guid employeeId, Guid orgId, Guid? performedByUserId = null);
}
