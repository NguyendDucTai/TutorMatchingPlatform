using FluentValidation;

namespace TutorMatchingPlatform.Application.Auth.Commands.SetupTutorProfile
{
    public class SetupTutorProfileCommandValidator : AbstractValidator<SetupTutorProfileCommand>
    {
        public SetupTutorProfileCommandValidator()
        {
            RuleFor(v => v.FullName)
                .NotEmpty().WithMessage("MSG02")
                .MaximumLength(50).WithMessage("Full Name cannot exceed 50 characters.");

            RuleFor(v => v.Subjects)
                .NotEmpty().WithMessage("At least 1 subject is required.");

            RuleForEach(v => v.Subjects).ChildRules(subjects =>
            {
                subjects.RuleFor(s => s.Rate)
                    .GreaterThan(0).WithMessage("Rate per Session must be a positive number.");
            });
        }
    }
}
