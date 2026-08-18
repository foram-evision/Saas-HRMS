namespace HRMS.Application.DTOs.Leave;

/// <summary>
/// Leave balance summary for one employee across all leave types for a given year.
/// Shows how many days are allowed, used, and remaining per leave type.
/// </summary>
public class LeaveBalanceDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<LeaveTypeBalanceDto> Balances { get; set; } = new();
}

/// <summary>Per-leave-type breakdown within the balance summary.</summary>
public class LeaveTypeBalanceDto
{
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int TotalAllowed { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
}
