using FluentValidation;
using HRMS.Application.DTOs.Department;

namespace HRMS.Application.Validators;

/// <summary>Validates incoming requests to update an existing department.</summary>
public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentDto>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(150).WithMessage("Department name cannot exceed 150 characters.")
            .Matches(@"^[\w\s\-&.]+$").WithMessage("Department name contains invalid characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ManagerId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ManagerId must be a valid non-empty GUID.");
    }
}
