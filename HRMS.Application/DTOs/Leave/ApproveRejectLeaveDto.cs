using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Leave;

/// <summary>Request DTO for HR/Admin approving or rejecting a leave request.</summary>
public class ApproveRejectLeaveDto
{
    /// <summary>
    /// Required when rejecting a leave request.
    /// Optional when approving.
    /// </summary>
    [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters.")]
    public string? RejectionReason { get; set; }
}
