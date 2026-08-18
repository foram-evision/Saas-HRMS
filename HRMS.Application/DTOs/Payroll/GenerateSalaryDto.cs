using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Payroll;

/// <summary>Request DTO for HR/Admin generating a monthly salary for one employee.</summary>
public class GenerateSalaryDto
{
    [Required(ErrorMessage = "Employee ID is required.")]
    public Guid EmployeeId { get; set; }

    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
    public int Month { get; set; }

    [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100.")]
    public int Year { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Basic salary must be a positive number.")]
    public decimal BasicSalary { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "HRA must be a positive number.")]
    public decimal HRA { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Bonus must be a positive number.")]
    public decimal Bonus { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Deductions must be a positive number.")]
    public decimal Deductions { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
