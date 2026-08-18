using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.Payroll;

/// <summary>Response DTO for a salary record returned in list views.</summary>
public class SalaryResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal Bonus { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }
    public SalaryStatus Status { get; set; }
    public string GeneratedByName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
}
