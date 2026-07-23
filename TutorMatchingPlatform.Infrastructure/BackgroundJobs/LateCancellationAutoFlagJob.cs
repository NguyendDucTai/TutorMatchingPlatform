using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Infrastructure.BackgroundJobs
{
    public class LateCancellationAutoFlagJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LateCancellationAutoFlagJob> _logger;

        public LateCancellationAutoFlagJob(IServiceProvider serviceProvider, ILogger<LateCancellationAutoFlagJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LateCancellationAutoFlagJob is starting.");

            // Run immediately on start, then every 24 hours
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessLateCancellationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Job retries on transient failure and logs errors (BR-04.2.4)
                    _logger.LogError(ex, "Error occurred executing LateCancellationAutoFlagJob.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task ProcessLateCancellationsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var thresholdDate = DateTime.UtcNow.AddDays(-30);

            // Group by User to count late cancellations in the last 30 days
            var offendingUsers = await context.CreditTransactions
                .Where(t => t.Type == CreditTransactionType.LateCancellationFee && t.CreatedAt >= thresholdDate)
                .GroupBy(t => t.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .Where(x => x.Count >= 3)
                .ToListAsync(cancellationToken);

            foreach (var offender in offendingUsers)
            {
                // Check if a LateCancellation complaint already exists for this user in the last 30 days
                bool alreadyFlagged = await context.Complaints
                    .AnyAsync(c => c.ReportedUserId == offender.UserId 
                                && c.Type == ComplaintType.LateCancellation 
                                && c.Source == ComplaintSource.SystemGenerated
                                && c.CreatedAt >= thresholdDate, cancellationToken);

                if (!alreadyFlagged)
                {
                    var complaint = new Complaint
                    {
                        ReportedUserId = offender.UserId,
                        Type = ComplaintType.LateCancellation,
                        Source = ComplaintSource.SystemGenerated,
                        Status = ComplaintStatus.Pending,
                        Description = $"System auto-flag: User has {offender.Count} late cancellations/reschedules in the rolling 30-day window."
                    };

                    context.Complaints.Add(complaint);
                    _logger.LogInformation($"Auto-flagged User {offender.UserId} for {offender.Count} late cancellations.");
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
