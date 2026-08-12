using FluentValidation;

namespace TutorPlatform.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Tài khoản không được để trống")
                .MaximumLength(100).WithMessage("Tài khoản không được vượt quá 100 ký tự")
                .EmailAddress().WithMessage("Email không đúng định dạng.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống")
                .MaximumLength(100).WithMessage("Mật khẩu không được vượt quá 100 ký tự")
                .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên không được để trống")
                .MaximumLength(50).WithMessage("Họ và tên không được vượt quá 50 ký tự.");

            RuleFor(x => x.Role)
                .Must(role => role == 1 || role == 2)
                .WithMessage("Vai trò không hợp lệ.");
        }
    }
}
