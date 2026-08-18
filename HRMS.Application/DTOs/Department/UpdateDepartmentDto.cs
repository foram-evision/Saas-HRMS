using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Department;

/// <summary>Request DTO for updating an existing department.</summary>
public class UpdateDepartmentDto
{
    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Optional: Updated manager. Pass null to remove the current manager.</summary>
    public Guid? ManagerId { get; set; }

    public bool IsActive { get; set; } = true;
}
