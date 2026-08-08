using MediatR;

namespace TutorPlatform.Application.Features.Auth.Commands.ResetPasswordWithOtp
{
    public class ResetPasswordWithOtpCommand : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
