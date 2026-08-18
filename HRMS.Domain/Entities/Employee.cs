using HRMS.Domain.Base;

namespace HRMS.Domain.Entities;

/// <summary>
/// Employee profile information.
/// Basic identity information (Name, Email, Phone, Login)
/// is stored in ApplicationUser.
/// This table stores only employee-specific details.
/// </summary>
public class Employee : BaseEntity
{
    /// <summary>
    /// Foreign Key to Users table.
    /// Every employee must have one user account.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Employee address.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Organization to which the employee belongs.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// HR/Admin who created this employee.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Employee First Name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Employee Last Name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Department to which this employee belongs. Nullable — not all employees
    /// are assigned to a department at the time of creation.
    /// </summary>
    public Guid? DepartmentId { get; set; }

    // Navigation Properties

    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ApplicationUser CreatedByUser { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;

    public virtual Department? Department { get; set; }
}