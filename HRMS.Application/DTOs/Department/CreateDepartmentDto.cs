using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Department;

/// <summary>Request DTO for creating a new department.</summary>
public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Optional: User ID of the department manager.</summary>
    public Guid? ManagerId { get; set; }
}
