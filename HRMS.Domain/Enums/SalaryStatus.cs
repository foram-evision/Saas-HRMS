namespace HRMS.Domain.Enums;

/// <summary>
/// Represents the processing status of a salary/payroll record.
/// </summary>
public enum SalaryStatus
{
    /// <summary>Salary record created but not yet finalized.</summary>
    Draft = 1,

    /// <summary>Salary has been generated and is ready for payment.</summary>
    Generated = 2,

    /// <summary>Salary has been paid to the employee.</summary>
    Paid = 3
}
