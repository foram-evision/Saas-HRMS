using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Leave;

/// <summary>Request DTO for an employee submitting a leave application.</summary>
public class ApplyLeaveDto
{
    [Required(ErrorMessage = "Leave type is required.")]
    public Guid LeaveTypeId { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    public DateOnly EndDate { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
    public string Reason { get; set; } = string.Empty;
}
