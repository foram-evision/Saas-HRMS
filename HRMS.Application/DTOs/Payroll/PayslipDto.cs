using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.Payroll;

/// <summary>
/// Detailed payslip DTO for a single employee's salary record.
/// Contains all salary components and is used for the payslip view/download endpoint.
/// </summary>
public class PayslipDto
{
    // ─── Employee Details ──────────────────────────────────────────────────────
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }

    // ─── Period ───────────────────────────────────────────────────────────────
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName { get; set; } = string.Empty; // "August 2025"

    // ─── Salary Breakdown ─────────────────────────────────────────────────────
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal Bonus { get; set; }
    public decimal GrossSalary { get; set; }   // BasicSalary + HRA + Bonus
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }

    // ─── Status ───────────────────────────────────────────────────────────────
    public SalaryStatus Status { get; set; }
    public string GeneratedByName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
}
