using FluentValidation;
using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Validators;

/// <summary>Validates incoming requests to apply for leave.</summary>
public class ApplyLeaveValidator : AbstractValidator<ApplyLeaveDto>
{
    public ApplyLeaveValidator()
    {
        RuleFor(x => x.LeaveTypeId)
            .NotEmpty().WithMessage("Leave Type is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start Date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End Date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
