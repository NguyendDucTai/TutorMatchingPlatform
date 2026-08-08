using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Services
{
    public class OverdueClassNotifierService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueClassNotifierService> _logger;

        public OverdueClassNotifierService(IServiceProvider serviceProvider, ILogger<OverdueClassNotifierService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OverdueClassNotifierService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndNotifyOverdueClassesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing OverdueClassNotifierService.");
                }

                // Scan every 2 minutes for testing and real-time warnings
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }

            _logger.LogInformation("OverdueClassNotifierService is stopping.");
        }

        private async Task CheckAndNotifyOverdueClassesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get Vietnam local current time (UTC+7)
            var vnNow = DateTime.UtcNow.AddHours(7);

            // Find confirmed bookings (Status = 1) that ended in the past (ScheduledEndAt < vnNow)
            var overdueBookings = await dbContext.Bookings
                .Include(b => b.Tutor)
                .Include(b => b.Student)
                .Include(b => b.Subject)
                .Where(b => b.Status == 1 && b.ScheduledEndAt < vnNow)
                .ToListAsync();

            foreach (var booking in overdueBookings)
            {
                // Check if we have already sent an OverdueClassWarning (Type = 10) for this booking to avoid duplicates
                var alreadyNotified = await dbContext.Notifications
                    .AnyAsync(n => n.RelatedEntityId == booking.Id && n.Type == 10);

                if (!alreadyNotified)
                {
                    _logger.LogInformation("Sending overdue class warning notification to tutor {TutorId} for booking {BookingId}", booking.TutorId, booking.Id);

                    var startTimeFormatted = booking.ScheduledStartAt.ToString("HH:mm dd/MM/yyyy");
                    var notification = new NotificationDataModel
                    {
                        Id = Guid.NewGuid(),
                        UserId = booking.TutorId,
                        Title = "Cảnh báo: Buổi học chưa hoàn thành",
                        Message = $"Buổi học môn {booking.Subject?.Name} với học sinh {booking.Student?.FullName} lúc {startTimeFormatted} đã kết thúc. Vui lòng cập nhật trạng thái Hoàn thành để nhận thanh toán.",
                        Type = 10, // OverdueClassWarning
                        RelatedEntityId = booking.Id,
                        RelatedEntityType = "Booking",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.Notifications.Add(notification);
                }
            }

            if (overdueBookings.Any())
            {
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
