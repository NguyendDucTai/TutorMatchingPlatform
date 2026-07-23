using MediatR;

namespace TutorMatchingPlatform.Application.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<ResetPasswordResult>
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
