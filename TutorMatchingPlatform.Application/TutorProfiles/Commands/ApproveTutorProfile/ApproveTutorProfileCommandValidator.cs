using FluentValidation;

namespace TutorMatchingPlatform.Application.TutorProfiles.Commands.ApproveTutorProfile
{
    public class ApproveTutorProfileCommandValidator : AbstractValidator<ApproveTutorProfileCommand>
    {
        public ApproveTutorProfileCommandValidator()
        {
            RuleFor(v => v.TutorProfileId)
                .GreaterThanOrEqualTo(0).WithMessage("TutorProfileId is invalid.");
        }
    }
}
