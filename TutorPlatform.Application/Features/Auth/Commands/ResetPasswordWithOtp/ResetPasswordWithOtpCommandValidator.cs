using FluentValidation;

namespace TutorPlatform.Application.Features.Auth.Commands.ResetPasswordWithOtp
{
    public class ResetPasswordWithOtpCommandValidator : AbstractValidator<ResetPasswordWithOtpCommand>
    {
        public ResetPasswordWithOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Vui lòng nhập địa chỉ Email.")
                .EmailAddress().WithMessage("Địa chỉ Email không đúng định dạng.");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("Vui lòng nhập Mã xác thực OTP.")
                .Length(6).WithMessage("Mã OTP phải có đúng 6 chữ số.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Vui lòng nhập Mật khẩu mới.")
                .MinimumLength(6).WithMessage("Mật khẩu mới phải có tối thiểu 6 ký tự.");
        }
    }
}
