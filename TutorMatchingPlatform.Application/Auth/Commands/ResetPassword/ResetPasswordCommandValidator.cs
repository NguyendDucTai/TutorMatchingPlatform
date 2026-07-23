using FluentValidation;

namespace TutorMatchingPlatform.Application.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(v => v.Token)
                .NotEmpty().WithMessage("Reset token is required.");

            RuleFor(v => v.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

            RuleFor(v => v.ConfirmPassword)
                .NotEmpty().WithMessage("Password confirmation is required.")
                .Equal(v => v.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}
