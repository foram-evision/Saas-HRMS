using HRMS.Application.Common;
using HRMS.Application.DTOs.Dashboard;
using HRMS.Application.Interfaces;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

/// <summary>
/// Aggregates organization metrics for the dashboard.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILeaveRepository _leaveRepository;
    private readonly IPayrollRepository _payrollRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public DashboardService(
        IEmployeeRepository employeeRepository,
        IAttendanceRepository attendanceRepository,
        ILeaveRepository leaveRepository,
        IPayrollRepository payrollRepository,
        IDepartmentRepository departmentRepository,
        IAuditLogRepository auditLogRepository)
    {
        _employeeRepository = employeeRepository;
        _attendanceRepository = attendanceRepository;
        _leaveRepository = leaveRepository;
        _payrollRepository = payrollRepository;
        _departmentRepository = departmentRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<ApiResponse<DashboardDto>> GetDashboardAsync(Guid orgId)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        int currentMonth = now.Month;
        int currentYear = now.Year;

        // Run independent queries concurrently
        var employeesTask = _employeeRepository.GetAllAsync(orgId, null);
        var departmentsTask = _departmentRepository.GetAllAsync(orgId);
        var todayAttendanceTask = _attendanceRepository.GetOrgDailyAsync(orgId, today);
        var pendingLeavesTask = _leaveRepository.GetPendingLeavesAsync(orgId);
        var orgPayrollTask = _payrollRepository.GetOrgPayrollAsync(orgId, currentMonth, currentYear);
        var recentAuditsTask = _auditLogRepository.GetRecentAsync(orgId, 10);
        // We can get all leaves and filter for today (to calculate EmployeesOnLeave)
        var allLeavesTask = _leaveRepository.GetAllLeavesAsync(orgId, LeaveStatus.Approved, 1, 1000);

        await Task.WhenAll(
            employeesTask, departmentsTask, todayAttendanceTask, 
            pendingLeavesTask, orgPayrollTask, recentAuditsTask, allLeavesTask);

        var employees = employeesTask.Result.ToList();
        var departments = departmentsTask.Result.ToList();
        var todayAttendance = todayAttendanceTask.Result.ToList();
        var pendingLeaves = pendingLeavesTask.Result.Count();
        var orgPayroll = orgPayrollTask.Result.ToList();
        var recentAudits = recentAuditsTask.Result;
        var approvedLeaves = allLeavesTask.Result.Items;

        // Employee stats
        int totalEmployees = employees.Count;
        int activeEmployees = employees.Count(e => e.User?.IsActive ?? true);
        int inactiveEmployees = totalEmployees - activeEmployees;

        // Department count
        int totalDepartments = departments.Count;

        // Today's attendance
        int presentToday = todayAttendance.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.HalfDay);
        int lateToday = todayAttendance.Count(a => a.Status == AttendanceStatus.Late);
        
        // Calculate employees on leave TODAY by checking active approved leaves
        int onLeaveToday = approvedLeaves.Count(l => l.StartDate <= today && l.EndDate >= today);
        
        int absentToday = Math.Max(0, totalEmployees - (presentToday + lateToday + onLeaveToday));

        // Payroll
        decimal totalPayrollCost = orgPayroll.Sum(s => s.NetSalary);
        int salaryGenerated = orgPayroll.Count;
        int salaryPaid = orgPayroll.Count(s => s.Status == SalaryStatus.Paid);

        // Recent activity
        var activityFeed = recentAudits.Select(a => new RecentAuditDto
        {
            Action = a.Action,
            EntityType = a.EntityType,
            PerformedBy = a.PerformedByUser?.FullName ?? "System",
            Timestamp = a.Timestamp
        }).ToList();

        var dashboard = new DashboardDto
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            InactiveEmployees = inactiveEmployees,
            TotalDepartments = totalDepartments,
            PresentToday = presentToday,
            AbsentToday = absentToday,
            LateToday = lateToday,
            OnLeaveToday = onLeaveToday,
            PendingLeaveRequests = pendingLeaves,
            TotalMonthlyPayrollCost = totalPayrollCost,
            EmployeesWithSalaryGenerated = salaryGenerated,
            EmployeesWithSalaryPaid = salaryPaid,
            EmployeesOnLeave = onLeaveToday,
            RecentActivity = activityFeed
        };

        return ApiResponse<DashboardDto>.SuccessResult(dashboard);
    }
}
