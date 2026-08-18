using HRMS.Domain.Base;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

/// <summary>
/// Represents a monthly salary record for an employee.
/// One record per employee per month+year combination.
/// NetSalary = BasicSalary + HRA + Bonus - Deductions.
/// </summary>
public class Salary : BaseEntity
{
    /// <summary>Employee this salary record belongs to.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Organization context for multi-tenant scoping.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Month for which this salary is generated (1–12).</summary>
    public int Month { get; set; }

    /// <summary>Year for which this salary is generated (e.g., 2025).</summary>
    public int Year { get; set; }

    /// <summary>Base salary component before allowances and deductions.</summary>
    public decimal BasicSalary { get; set; }

    /// <summary>House Rent Allowance or any housing component.</summary>
    public decimal HRA { get; set; }

    /// <summary>Performance bonus or incentive for this period.</summary>
    public decimal Bonus { get; set; }

    /// <summary>Total deductions (taxes, PF, absence penalties, etc.).</summary>
    public decimal Deductions { get; set; }

    /// <summary>
    /// Net take-home salary.
    /// Calculated as: BasicSalary + HRA + Bonus - Deductions.
    /// </summary>
    public decimal NetSalary { get; set; }

    /// <summary>Processing status of this salary record.</summary>
    public SalaryStatus Status { get; set; } = SalaryStatus.Draft;

    /// <summary>User ID of the HR/Admin who generated this salary record.</summary>
    public Guid GeneratedBy { get; set; }

    /// <summary>UTC timestamp when this salary record was created.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when the salary was marked as paid. Null if not yet paid.</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Optional notes (e.g., "Includes Q3 performance bonus").</summary>
    public string? Notes { get; set; }

    // ─── Navigation Properties ─────────────────────────────────────────────────

    public virtual Employee Employee { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual ApplicationUser GeneratedByUser { get; set; } = null!;
}
