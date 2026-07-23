using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Infrastructure.BackgroundJobs
{
    public class SessionReminderJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SessionReminderJob> _logger;

        public SessionReminderJob(IServiceProvider serviceProvider, ILogger<SessionReminderJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SessionReminderJob started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        var now = DateTime.UtcNow;
                        var reminderWindowStart = now;
                        var reminderWindowEnd = now.AddHours(1).AddMinutes(5); // Search up to slightly over 1 hour to ensure we catch them

                        var sessionsToRemind = await context.Sessions
                            .Include(s => s.Tutor).ThenInclude(t => t.User)
                            .Include(s => s.Student).ThenInclude(st => st.User)
                            .Where(s => s.Status == SessionStatus.Confirmed 
                                     && !s.ReminderSent 
                                     && s.StartTime >= reminderWindowStart 
                                     && s.StartTime <= reminderWindowEnd)
                            .ToListAsync(stoppingToken);

                        foreach (var session in sessionsToRemind)
                        {
                            var tutorEmail = session.Tutor.User.Email;
                            var studentEmail = session.Student.User.Email;
                            var subject = "Session Reminder";
                            var body = $"Your session for subject {session.SubjectId} is starting in less than 1 hour. Get ready!";

                            await emailService.SendEmailAsync(tutorEmail, subject, body);
                            await emailService.SendEmailAsync(studentEmail, subject, body);

                            session.ReminderSent = true;
                        }

                        if (sessionsToRemind.Any())
                        {
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"Sent reminders for {sessionsToRemind.Count} sessions.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing SessionReminderJob.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
