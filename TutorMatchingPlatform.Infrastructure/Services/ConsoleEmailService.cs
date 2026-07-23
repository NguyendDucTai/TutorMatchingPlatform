using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Infrastructure.Services
{
    /// <summary>
    /// Console-based email service implementation for development/testing.
    /// Replace with a real implementation (SMTP, SendGrid, etc.) in production.
    /// </summary>
    public class ConsoleEmailService : IEmailService
    {
        private readonly ILogger<ConsoleEmailService> _logger;

        public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            _logger.LogInformation(
                "[EMAIL SERVICE] Password reset token for {Email}: {Token}",
                toEmail,
                resetToken);

            // In production, replace with actual SMTP or third-party email service:
            // Example:
            //   var resetLink = $"https://yourapp.com/reset-password?token={resetToken}";
            //   await _smtpClient.SendMailAsync(new MailMessage(...));

            return Task.CompletedTask;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation(
                "[EMAIL SERVICE] To: {Email} | Subject: {Subject} | Body: {Body}",
                toEmail,
                subject,
                body);

            return Task.CompletedTask;
        }
    }
}
