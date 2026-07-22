using FluentValidation;

namespace TutorMatchingPlatform.Application.Complaints.Commands.SubmitComplaint
{
    public class SubmitComplaintCommandValidator : AbstractValidator<SubmitComplaintCommand>
    {
        public SubmitComplaintCommandValidator()
        {
            RuleFor(v => v.ReportedUserId).GreaterThan(0);
            RuleFor(v => v.Type).IsInEnum();
            RuleFor(v => v.Description)
                .NotEmpty().WithMessage("MSG02")
                .MaximumLength(500).WithMessage("MSG02");
        }
    }
}
