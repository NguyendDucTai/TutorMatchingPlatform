using FluentValidation;

namespace TutorMatchingPlatform.Application.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
    {
        public UpdateMyProfileCommandValidator()
        {
            RuleFor(v => v.UserId).GreaterThan(0);
            RuleFor(v => v.FullName).NotEmpty().MaximumLength(100);
            RuleFor(v => v.Bio).MaximumLength(1000);
            RuleFor(v => v.StudyGoals).MaximumLength(1000);
        }
    }
}
