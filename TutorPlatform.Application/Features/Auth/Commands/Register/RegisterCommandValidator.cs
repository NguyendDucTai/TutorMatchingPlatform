using FluentValidation;

namespace TutorPlatform.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be valid.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required.");

            RuleFor(x => x.Role)
                .Must(role => role == 1 || role == 2)
                .WithMessage("Role must be 1 (Tutor) or 2 (Student).");
        }
    }
}
