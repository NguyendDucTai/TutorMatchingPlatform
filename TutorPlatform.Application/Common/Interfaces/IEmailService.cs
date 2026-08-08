using System.Threading.Tasks;

namespace TutorPlatform.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlContent);
    }
}
