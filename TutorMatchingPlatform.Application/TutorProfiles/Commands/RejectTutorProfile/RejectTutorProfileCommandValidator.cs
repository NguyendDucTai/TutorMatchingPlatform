using FluentValidation;

namespace TutorMatchingPlatform.Application.TutorProfiles.Commands.RejectTutorProfile
{
    public class RejectTutorProfileCommandValidator : AbstractValidator<RejectTutorProfileCommand>
    {
        public RejectTutorProfileCommandValidator()
        {
            RuleFor(v => v.TutorProfileId)
                .GreaterThanOrEqualTo(0).WithMessage("TutorProfileId is invalid.");

            RuleFor(v => v.Reason)
                .NotEmpty().WithMessage("Reason is required to reject a profile.")
                .MaximumLength(200).WithMessage("Reason must not exceed 200 characters.");
        }
    }
}
