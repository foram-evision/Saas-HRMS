namespace HRMS.Application.DTOs.Dashboard;

/// <summary>
/// Main dashboard DTO aggregating key HRMS metrics for an organization.
/// Returned by GET /api/dashboard and scoped to the calling user's organization.
/// </summary>
public class DashboardDto
{
    // ─── Employee Stats ────────────────────────────────────────────────────────
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }
    public int TotalDepartments { get; set; }

    // ─── Today's Attendance ───────────────────────────────────────────────────
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public int LateToday { get; set; }
    public int OnLeaveToday { get; set; }

    // ─── Leave Stats ──────────────────────────────────────────────────────────
    public int PendingLeaveRequests { get; set; }
    public int ApprovedLeavesThisMonth { get; set; }
    public int EmployeesOnLeave { get; set; }

    // ─── Payroll Stats ────────────────────────────────────────────────────────
    public decimal TotalMonthlyPayrollCost { get; set; }
    public int EmployeesWithSalaryGenerated { get; set; }
    public int EmployeesWithSalaryPaid { get; set; }

    // ─── Recent Activity ──────────────────────────────────────────────────────
    public List<RecentAuditDto> RecentActivity { get; set; } = new();
}

/// <summary>Compact audit event entry for the dashboard activity feed.</summary>
public class RecentAuditDto
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
