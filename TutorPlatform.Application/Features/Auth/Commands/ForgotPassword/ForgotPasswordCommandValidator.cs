using FluentValidation;

namespace TutorPlatform.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Vui lòng nhập địa chỉ Email.")
                .EmailAddress().WithMessage("Địa chỉ Email không đúng định dạng.");
        }
    }
}
