using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Department>> GetAllAsync(Guid organizationId)
    {
        return await _context.Set<Department>()
            .Include(d => d.Manager)
            .Where(d => d.OrganizationId == organizationId && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(Guid id, Guid organizationId)
    {
        return await _context.Set<Department>()
            .Include(d => d.Manager)
            .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == organizationId);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid organizationId, Guid? excludeId = null)
    {
        return await _context.Set<Department>()
            .AnyAsync(d => d.OrganizationId == organizationId &&
                           d.Name.ToLower() == name.ToLower() &&
                           d.IsActive &&
                           (excludeId == null || d.Id != excludeId));
    }

    public async Task<Department> AddAsync(Department department)
    {
        await _context.Set<Department>().AddAsync(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department> UpdateAsync(Department department)
    {
        _context.Set<Department>().Update(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, Guid organizationId)
    {
        var dept = await GetByIdAsync(id, organizationId);
        if (dept is null) return false;

        dept.IsActive = false;
        dept.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetEmployeeCountAsync(Guid departmentId)
    {
        return await _context.Set<Employee>()
            .CountAsync(e => e.DepartmentId == departmentId);
    }

    public async Task<bool> AssignEmployeeAsync(Guid employeeId, Guid departmentId, Guid organizationId)
    {
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.OrganizationId == organizationId);

        if (employee == null) return false;

        var department = await GetByIdAsync(departmentId, organizationId);
        if (department == null) return false;

        employee.DepartmentId = departmentId;
        await _context.SaveChangesAsync();
        return true;
    }
}
