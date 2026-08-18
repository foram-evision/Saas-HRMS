using HRMS.Application.Common;
using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Interfaces;

/// <summary>
/// Application service contract for payroll management operations.
/// </summary>
public interface IPayrollService
{
    /// <summary>
    /// Generate salary for one employee for a given month and year.
    /// Returns an error if salary already generated for this period.
    /// </summary>
    Task<ApiResponse<SalaryResponseDto>> GenerateSalaryAsync(GenerateSalaryDto dto, Guid orgId, Guid generatedByUserId);

    /// <summary>
    /// Get detailed payslip for a single salary record.
    /// Employees can only access their own payslip; HR/Admin can access any.
    /// </summary>
    Task<ApiResponse<PayslipDto>> GetPayslipAsync(Guid salaryId, Guid requestingEmployeeId, bool isHrOrAdmin);

    /// <summary>Get personal payroll history for the authenticated employee.</summary>
    Task<ApiResponse<List<SalaryResponseDto>>> GetMyPayrollHistoryAsync(Guid employeeId);

    /// <summary>Get full payroll run for an org in a given month+year. HR/Admin only.</summary>
    Task<ApiResponse<List<SalaryResponseDto>>> GetOrgPayrollAsync(Guid orgId, int month, int year);

    /// <summary>Mark a salary record as Paid. HR/Admin only.</summary>
    Task<ApiResponse<SalaryResponseDto>> MarkAsPaidAsync(Guid salaryId, Guid updatedByUserId);
}
