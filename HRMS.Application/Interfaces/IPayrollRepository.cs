using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Data access contract for payroll/salary records.
/// Implemented by PayrollRepository in the Infrastructure layer.
/// </summary>
public interface IPayrollRepository
{
    /// <summary>Check whether a salary record already exists for a given employee+month+year.</summary>
    Task<bool> ExistsAsync(Guid employeeId, int month, int year);

    Task<Salary> AddAsync(Salary salary);
    Task<Salary> UpdateAsync(Salary salary);
    Task<Salary?> GetByIdAsync(Guid id);

    /// <summary>Get all salary records for a specific employee (history).</summary>
    Task<IEnumerable<Salary>> GetByEmployeeAsync(Guid employeeId);

    /// <summary>Get all salary records for an organization in a given month+year (payroll run).</summary>
    Task<IEnumerable<Salary>> GetOrgPayrollAsync(Guid organizationId, int month, int year);

    /// <summary>Get total net salary cost for an organization in a given month+year.</summary>
    Task<decimal> GetTotalMonthlyCostAsync(Guid organizationId, int month, int year);
}
