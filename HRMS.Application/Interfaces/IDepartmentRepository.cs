using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Data access contract for department management.
/// Implemented by DepartmentRepository in the Infrastructure layer.
/// </summary>
public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetAllAsync(Guid organizationId);
    Task<Department?> GetByIdAsync(Guid id, Guid organizationId);
    Task<bool> ExistsByNameAsync(string name, Guid organizationId, Guid? excludeId = null);
    Task<Department> AddAsync(Department department);
    Task<Department> UpdateAsync(Department department);

    /// <summary>Soft-delete a department by setting IsActive = false.</summary>
    Task<bool> SoftDeleteAsync(Guid id, Guid organizationId);

    /// <summary>Count employees assigned to a department.</summary>
    Task<int> GetEmployeeCountAsync(Guid departmentId);

    /// <summary>Assigns an employee to a specific department.</summary>
    Task<bool> AssignEmployeeAsync(Guid employeeId, Guid departmentId, Guid organizationId);
}
