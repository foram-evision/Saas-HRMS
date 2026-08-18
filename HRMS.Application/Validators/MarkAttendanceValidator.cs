using FluentValidation;
using HRMS.Application.DTOs.Attendance;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators;

/// <summary>Validates incoming requests for HR to manually mark attendance.</summary>
public class MarkAttendanceValidator : AbstractValidator<MarkAttendanceDto>
{
    public MarkAttendanceValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Cannot mark attendance for future dates.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid attendance status.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");

        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn)
            .When(x => x.CheckIn.HasValue && x.CheckOut.HasValue)
            .WithMessage("Check-out time must be after check-in time.");
    }
}
