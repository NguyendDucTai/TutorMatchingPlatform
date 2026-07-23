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

namespace TutorMatchingPlatform.API.BackgroundServices
{
    public class LateCancellationFlagWorker : BackgroundService
    {
        private readonly ILogger<LateCancellationFlagWorker> _logger;
        private readonly ILateCancellationQueue _queue;
        private readonly IServiceProvider _serviceProvider;

        public LateCancellationFlagWorker(ILogger<LateCancellationFlagWorker> logger, ILateCancellationQueue queue, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _queue = queue;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LateCancellationFlagWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    int userId = await _queue.DequeueAsync(stoppingToken);

                    // Retry logic for transient failures (BR-08 / 4.2.4)
                    int maxRetries = 3;
                    for (int attempt = 1; attempt <= maxRetries; attempt++)
                    {
                        try
                        {
                            await ProcessUserCancellationCheckAsync(userId, stoppingToken);
                            break; // Success
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error processing late cancellation for User {UserId}. Attempt {Attempt} of {MaxRetries}", userId, attempt, maxRetries);
                            if (attempt == maxRetries)
                            {
                                _logger.LogError(ex, "Final attempt failed for User {UserId}.", userId);
                            }
                            else
                            {
                                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), stoppingToken); // Exponential backoff
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stopping token was canceled
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing LateCancellationFlagWorker.");
                }
            }
        }

        private async Task ProcessUserCancellationCheckAsync(int userId, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            // Count occurrences of late cancellations (<24h). 
            var lateCount = await dbContext.CreditTransactions
                .CountAsync(ct => ct.UserId == userId 
                               && ct.Type == CreditTransactionType.LateCancellationFee
                               && ct.CreatedAt >= thirtyDaysAgo, cancellationToken);

            if (lateCount >= 3)
            {
                // Check if a system-generated complaint already exists in the last 30 days to avoid spam
                bool hasRecentComplaint = await dbContext.Complaints
                    .AnyAsync(c => c.ReportedUserId == userId
                                && c.Source == ComplaintSource.SystemGenerated
                                && c.Type == ComplaintType.LateCancellation
                                && c.CreatedAt >= thirtyDaysAgo, cancellationToken);

                if (!hasRecentComplaint)
                {
                    var complaint = new Complaint
                    {
                        ReportedUserId = userId,
                        Type = ComplaintType.LateCancellation,
                        Source = ComplaintSource.SystemGenerated,
                        Description = $"System Flag: User has exceeded the allowed limit of late cancellations/reschedules (<24h). Total in last 30 days: {lateCount}.",
                        Status = ComplaintStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.Complaints.Add(complaint);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("System generated a LateCancellation complaint for User {UserId}. Count: {Count}", userId, lateCount);
                }
            }
        }
    }
}
