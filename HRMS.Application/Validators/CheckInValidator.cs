using FluentValidation;
using HRMS.Application.DTOs.Attendance;

namespace HRMS.Application.Validators;

/// <summary>Validates incoming requests for employee check-in.</summary>
public class CheckInValidator : AbstractValidator<CheckInDto>
{
    public CheckInValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
