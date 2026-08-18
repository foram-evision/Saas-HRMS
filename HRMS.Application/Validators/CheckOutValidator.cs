using FluentValidation;
using HRMS.Application.DTOs.Attendance;

namespace HRMS.Application.Validators;

/// <summary>Validates incoming requests for employee check-out.</summary>
public class CheckOutValidator : AbstractValidator<CheckOutDto>
{
    public CheckOutValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
