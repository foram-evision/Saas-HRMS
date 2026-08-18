using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Leave;

/// <summary>Response DTO for a leave type record.</summary>
public class LeaveTypeResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalDaysAllowed { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Request DTO for creating a new leave type.</summary>
public class CreateLeaveTypeDto
{
    [Required(ErrorMessage = "Leave type name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, 365, ErrorMessage = "Total days allowed must be between 1 and 365.")]
    public int TotalDaysAllowed { get; set; }
}
