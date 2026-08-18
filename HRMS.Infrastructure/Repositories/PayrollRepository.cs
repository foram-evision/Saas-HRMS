using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class PayrollRepository : IPayrollRepository
{
    private readonly ApplicationDbContext _context;

    public PayrollRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid employeeId, int month, int year)
    {
        return await _context.Set<Salary>()
            .AnyAsync(s => s.EmployeeId == employeeId && s.Month == month && s.Year == year);
    }

    public async Task<Salary> AddAsync(Salary salary)
    {
        await _context.Set<Salary>().AddAsync(salary);
        await _context.SaveChangesAsync();
        return salary;
    }

    public async Task<Salary> UpdateAsync(Salary salary)
    {
        _context.Set<Salary>().Update(salary);
        await _context.SaveChangesAsync();
        return salary;
    }

    public async Task<Salary?> GetByIdAsync(Guid id)
    {
        return await _context.Set<Salary>()
            .Include(s => s.Employee)
                .ThenInclude(e => e.User)
            .Include(s => s.Employee)
                .ThenInclude(e => e.Department)
            .Include(s => s.Organization)
            .Include(s => s.GeneratedByUser)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Salary>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.Set<Salary>()
            .Include(s => s.Employee)
                .ThenInclude(e => e.User)
            .Include(s => s.GeneratedByUser)
            .Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync();
    }

    public async Task<IEnumerable<Salary>> GetOrgPayrollAsync(Guid organizationId, int month, int year)
    {
        return await _context.Set<Salary>()
            .Include(s => s.Employee)
                .ThenInclude(e => e.User)
            .Include(s => s.GeneratedByUser)
            .Where(s => s.OrganizationId == organizationId && s.Month == month && s.Year == year)
            .OrderBy(s => s.Employee.User.FullName)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalMonthlyCostAsync(Guid organizationId, int month, int year)
    {
        return await _context.Set<Salary>()
            .Where(s => s.OrganizationId == organizationId && s.Month == month && s.Year == year)
            .SumAsync(s => s.NetSalary);
    }
}
