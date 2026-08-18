using HRMS.Application.Common;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

/// <summary>
/// Business logic for payroll/salary management.
/// NetSalary = BasicSalary + HRA + Bonus - Deductions.
/// </summary>
public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository  _payrollRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAuditLogService    _auditLogService;

    public PayrollService(
        IPayrollRepository  payrollRepository,
        IEmployeeRepository employeeRepository,
        IAuditLogService    auditLogService)
    {
        _payrollRepository  = payrollRepository;
        _employeeRepository = employeeRepository;
        _auditLogService    = auditLogService;
    }

    // ─── Generate Salary ──────────────────────────────────────────────────────

    public async Task<ApiResponse<SalaryResponseDto>> GenerateSalaryAsync(
        GenerateSalaryDto dto, Guid orgId, Guid generatedByUserId)
    {
        // Prevent duplicate generation for the same period
        var exists = await _payrollRepository.ExistsAsync(dto.EmployeeId, dto.Month, dto.Year);
        if (exists)
            return ApiResponse<SalaryResponseDto>.Failure(
                $"Salary for this employee has already been generated for {dto.Month}/{dto.Year}.");

        var netSalary = dto.BasicSalary + dto.HRA + dto.Bonus - dto.Deductions;
        if (netSalary < 0)
            return ApiResponse<SalaryResponseDto>.Failure("Net salary cannot be negative. Check deductions.");

        var salary = new Salary
        {
            EmployeeId     = dto.EmployeeId,
            OrganizationId = orgId,
            Month          = dto.Month,
            Year           = dto.Year,
            BasicSalary    = dto.BasicSalary,
            HRA            = dto.HRA,
            Bonus          = dto.Bonus,
            Deductions     = dto.Deductions,
            NetSalary      = netSalary,
            Status         = SalaryStatus.Generated,
            GeneratedBy    = generatedByUserId,
            GeneratedAt    = DateTime.UtcNow,
            Notes          = dto.Notes
        };

        var saved = await _payrollRepository.AddAsync(salary);

        await _auditLogService.LogAsync(
            generatedByUserId, orgId,
            AppConstants.AuditEvents.SalaryGenerated, "Salary", saved.Id,
            newValues: $"{{\"EmployeeId\":\"{dto.EmployeeId}\",\"Month\":{dto.Month},\"Year\":{dto.Year},\"NetSalary\":{netSalary}}}");

        return ApiResponse<SalaryResponseDto>.SuccessResult(
            MapToDto(saved, string.Empty, string.Empty),
            "Salary generated successfully.");
    }

    // ─── Get Payslip ──────────────────────────────────────────────────────────

    public async Task<ApiResponse<PayslipDto>> GetPayslipAsync(
        Guid salaryId, Guid requestingEmployeeId, bool isHrOrAdmin)
    {
        var salary = await _payrollRepository.GetByIdAsync(salaryId);
        if (salary is null)
            return ApiResponse<PayslipDto>.Failure("Salary record not found.");

        // Employees can only see their own payslip
        if (!isHrOrAdmin && salary.EmployeeId != requestingEmployeeId)
            return ApiResponse<PayslipDto>.Failure(AppConstants.Messages.Unauthorized);

        var monthName = new DateTime(salary.Year, salary.Month, 1).ToString("MMMM yyyy");

        var payslip = new PayslipDto
        {
            EmployeeId       = salary.EmployeeId,
            EmployeeName     = salary.Employee?.User?.FullName ?? string.Empty,
            EmployeeEmail    = salary.Employee?.User?.Email ?? string.Empty,
            OrganizationName = salary.Organization?.Name ?? string.Empty,
            DepartmentName   = salary.Employee?.Department?.Name,
            Month            = salary.Month,
            Year             = salary.Year,
            MonthName        = monthName,
            BasicSalary      = salary.BasicSalary,
            HRA              = salary.HRA,
            Bonus            = salary.Bonus,
            GrossSalary      = salary.BasicSalary + salary.HRA + salary.Bonus,
            Deductions       = salary.Deductions,
            NetSalary        = salary.NetSalary,
            Status           = salary.Status,
            GeneratedByName  = salary.GeneratedByUser?.FullName ?? string.Empty,
            GeneratedAt      = salary.GeneratedAt,
            PaidAt           = salary.PaidAt,
            Notes            = salary.Notes
        };

        return ApiResponse<PayslipDto>.SuccessResult(payslip);
    }

    // ─── My Payroll History ───────────────────────────────────────────────────

    public async Task<ApiResponse<List<SalaryResponseDto>>> GetMyPayrollHistoryAsync(Guid employeeId)
    {
        var records = await _payrollRepository.GetByEmployeeAsync(employeeId);
        var dtos    = records.Select(s => MapToDto(s, s.Employee?.User?.FullName ?? string.Empty,
                                                      s.GeneratedByUser?.FullName ?? string.Empty)).ToList();
        return ApiResponse<List<SalaryResponseDto>>.SuccessResult(dtos);
    }

    // ─── Org Payroll ──────────────────────────────────────────────────────────

    public async Task<ApiResponse<List<SalaryResponseDto>>> GetOrgPayrollAsync(Guid orgId, int month, int year)
    {
        var records = await _payrollRepository.GetOrgPayrollAsync(orgId, month, year);
        var dtos    = records.Select(s => MapToDto(s, s.Employee?.User?.FullName ?? string.Empty,
                                                      s.GeneratedByUser?.FullName ?? string.Empty)).ToList();
        return ApiResponse<List<SalaryResponseDto>>.SuccessResult(dtos);
    }

    // ─── Mark As Paid ─────────────────────────────────────────────────────────

    public async Task<ApiResponse<SalaryResponseDto>> MarkAsPaidAsync(Guid salaryId, Guid updatedByUserId)
    {
        var salary = await _payrollRepository.GetByIdAsync(salaryId);
        if (salary is null)
            return ApiResponse<SalaryResponseDto>.Failure("Salary record not found.");

        if (salary.Status == SalaryStatus.Paid)
            return ApiResponse<SalaryResponseDto>.Failure("Salary has already been marked as paid.");

        salary.Status = SalaryStatus.Paid;
        salary.PaidAt = DateTime.UtcNow;

        var updated = await _payrollRepository.UpdateAsync(salary);

        await _auditLogService.LogAsync(
            updatedByUserId, salary.OrganizationId,
            AppConstants.AuditEvents.SalaryPaid, "Salary", salaryId);

        return ApiResponse<SalaryResponseDto>.SuccessResult(
            MapToDto(updated, updated.Employee?.User?.FullName ?? string.Empty,
                              updated.GeneratedByUser?.FullName ?? string.Empty),
            "Salary marked as paid.");
    }

    // ─── Mapper ───────────────────────────────────────────────────────────────

    private static SalaryResponseDto MapToDto(Salary s, string employeeName, string generatedByName) => new()
    {
        Id              = s.Id,
        EmployeeId      = s.EmployeeId,
        EmployeeName    = employeeName,
        Month           = s.Month,
        Year            = s.Year,
        BasicSalary     = s.BasicSalary,
        HRA             = s.HRA,
        Bonus           = s.Bonus,
        Deductions      = s.Deductions,
        NetSalary       = s.NetSalary,
        Status          = s.Status,
        GeneratedByName = generatedByName,
        GeneratedAt     = s.GeneratedAt,
        PaidAt          = s.PaidAt,
        Notes           = s.Notes
    };
}
