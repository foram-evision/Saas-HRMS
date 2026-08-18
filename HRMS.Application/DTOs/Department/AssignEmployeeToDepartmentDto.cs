namespace HRMS.Application.DTOs.Department;

/// <summary>Request DTO to assign a single employee to a department.</summary>
public class AssignEmployeeToDepartmentDto
{
    /// <summary>The employee to assign to the department.</summary>
    public Guid EmployeeId { get; set; }
}
