using HRMS.Domain.Base;

namespace HRMS.Domain.Entities;

/// <summary>
/// Represents a department within an organization.
/// Employees can be assigned to departments for grouping and reporting.
/// </summary>
public class Department : BaseEntity
{
    /// <summary>Name of the department (e.g., "Engineering", "HR", "Finance").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description of the department's function.</summary>
    public string? Description { get; set; }

    /// <summary>Organization this department belongs to (tenant-scoped).</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Optional: User ID of the department manager.
    /// Null if no manager is assigned.
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>Indicates whether this department is currently active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>UTC timestamp when this department was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when this department was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ─────────────────────────────────────────────────

    public virtual Organization Organization { get; set; } = null!;
    public virtual ApplicationUser? Manager { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
