using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Infrastructure.BackgroundJobs
{
    public class ReputationScoreCalculationJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReputationScoreCalculationJob> _logger;

        public ReputationScoreCalculationJob(IServiceProvider serviceProvider, ILogger<ReputationScoreCalculationJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReputationScoreCalculationJob is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RecalculateScoresAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing ReputationScoreCalculationJob.");
                }

                // Run nightly (every 24 hours)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task RecalculateScoresAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var thresholdDate = DateTime.UtcNow.AddDays(-90);

            // Fetch all tutor profiles and recalculate based on last 90 days feedbacks
            var tutors = await context.Users
                .Include(u => u.TutorProfile)
                .Where(u => u.Role == Domain.Enums.UserRole.Tutor && u.TutorProfile != null)
                .Select(u => u.TutorProfile)
                .ToListAsync(cancellationToken);

            foreach (var profile in tutors)
            {
                if (profile == null) continue;

                var feedbacks = await context.Feedbacks
                    .Include(f => f.Session)
                    .Where(f => f.Session.TutorId == profile.UserId 
                             && f.SenderId != profile.UserId 
                             && f.CreatedAt >= thresholdDate)
                    .ToListAsync(cancellationToken);

                if (feedbacks.Any())
                {
                    // Weighted average (simple average for now if no specific weights are given)
                    profile.ReputationScore = Math.Round(feedbacks.Average(f => f.Rating), 2);
                }
                else
                {
                    profile.ReputationScore = 0.0;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Recalculated reputation scores for {tutors.Count} tutors.");
        }
    }
}
