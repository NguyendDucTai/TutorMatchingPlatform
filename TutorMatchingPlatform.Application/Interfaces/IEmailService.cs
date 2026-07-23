using System.Threading.Tasks;

namespace TutorMatchingPlatform.Application.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends a password reset email to the user.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="resetToken">The password reset token</param>
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
        
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
