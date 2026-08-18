using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.Leave;

/// <summary>Response DTO for a leave request record.</summary>
public class LeaveRequestResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; }
    public string? ApprovedByName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ActionedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
