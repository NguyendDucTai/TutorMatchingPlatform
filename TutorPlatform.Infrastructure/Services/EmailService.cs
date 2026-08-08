using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Infrastructure.Configurations;

namespace TutorPlatform.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly EmailSettings _emailSettings;
        private readonly IConfiguration _configuration;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            IConfiguration configuration)
        {
            _emailSettings = emailSettings.Value;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email cannot be empty.");

            // Output log for development / debugging
            Console.WriteLine($"=================================================");
            Console.WriteLine($"[EMAIL SERVICE] Sending Email to: {toEmail}");
            Console.WriteLine($"[EMAIL SERVICE] Subject: {subject}");
            Console.WriteLine($"=================================================");

            string brevoApiKey = _configuration["Brevo:ApiKey"]?.Trim() ?? string.Empty;

            // 1. Send via Brevo (Sendinblue) REST API
            if (!string.IsNullOrWhiteSpace(brevoApiKey) && brevoApiKey.StartsWith("xkeysib-"))
            {
                try
                {
                    var senderEmail = !string.IsNullOrWhiteSpace(_emailSettings.FromEmail) 
                        ? _emailSettings.FromEmail 
                        : "nguyenductai08102004ok@gmail.com";

                    var senderName = !string.IsNullOrWhiteSpace(_emailSettings.FromName) 
                        ? _emailSettings.FromName 
                        : "TutorMatching Platform";

                    var payload = new
                    {
                        sender = new { name = senderName, email = senderEmail },
                        to = new[] { new { email = toEmail } },
                        subject = subject,
                        htmlContent = htmlContent
                    };

                    using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
                    request.Headers.Add("api-key", brevoApiKey);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[BREVO SUCCESS] Email sent successfully to {toEmail} via Brevo REST API!");
                        return;
                    }
                    else
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[BREVO ERROR] Brevo API status {response.StatusCode}: {errorBody}");
                        throw new InvalidOperationException($"Không thể gửi Email qua Brevo: {response.StatusCode} - {errorBody}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BREVO EXCEPTION] {ex.Message}");
                    throw;
                }
            }

            // 2. Dev Fallback: If Brevo ApiKey is empty, log to Console
            Console.WriteLine($"[DEV NOTICE] Brevo ApiKey is not configured. Email logged to Console.");
        }
    }
}
