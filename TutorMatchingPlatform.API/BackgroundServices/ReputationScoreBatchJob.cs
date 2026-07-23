using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.API.BackgroundServices
{
    public class ReputationScoreBatchJob : BackgroundService
    {
        private readonly ILogger<ReputationScoreBatchJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ReputationScoreBatchJob(ILogger<ReputationScoreBatchJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReputationScoreBatchJob is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                // Schedule for 2:00 AM
                var nextRunTime = now.Date.AddHours(2);
                if (now > nextRunTime)
                {
                    nextRunTime = nextRunTime.AddDays(1);
                }

                var delay = nextRunTime - now;
                _logger.LogInformation("ReputationScoreBatchJob will run at: {time}", nextRunTime);

                try
                {
                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested) break;

                    _logger.LogInformation("ReputationScoreBatchJob is executing.");

                    using var scope = _serviceProvider.CreateScope();
                    var reputationService = scope.ServiceProvider.GetRequiredService<IReputationScoreService>();

                    await reputationService.RecalculateAllActiveUsersAsync(stoppingToken);

                    _logger.LogInformation("ReputationScoreBatchJob executed successfully.");
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing ReputationScoreBatchJob.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}
