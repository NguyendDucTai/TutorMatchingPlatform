using FluentValidation;

namespace TutorMatchingPlatform.Application.TutorProfiles.Commands.ApproveTutorProfile
{
    public class ApproveTutorProfileCommandValidator : AbstractValidator<ApproveTutorProfileCommand>
    {
        public ApproveTutorProfileCommandValidator()
        {
            RuleFor(v => v.TutorProfileId)
                .NotEmpty().WithMessage("TutorProfileId is required.");
        }
    }
}
