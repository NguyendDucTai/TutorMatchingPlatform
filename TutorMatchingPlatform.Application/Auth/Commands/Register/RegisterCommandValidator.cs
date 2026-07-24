using FluentValidation;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(v => v.Role)
                .Must(role => role == UserRole.Student || role == UserRole.Tutor)
                .WithMessage("Registering Role required: Tutor or Student.");

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .EmailAddress().WithMessage("Email format is invalid.");

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

            RuleFor(v => v.ConfirmPassword)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .Equal(v => v.Password).WithMessage("Confirm Password must match Password.");
        }
    }
}
