using MediatR;

namespace TutorMatchingPlatform.Application.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<ForgotPasswordResult>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Only populated in development/debug mode or when no email service is configured.
        /// In production, the token is sent via email.
        /// </summary>
        public string? ResetToken { get; set; }
    }
}
