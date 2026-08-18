using FluentValidation;
using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Validators;

/// <summary>Validates incoming requests to create a new leave type.</summary>
public class CreateLeaveTypeValidator : AbstractValidator<CreateLeaveTypeDto>
{
    public CreateLeaveTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Leave type name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.TotalDaysAllowed)
            .GreaterThan(0).WithMessage("Total days allowed must be greater than 0.")
            .LessThanOrEqualTo(365).WithMessage("Total days allowed cannot exceed a full year.");
    }
}
