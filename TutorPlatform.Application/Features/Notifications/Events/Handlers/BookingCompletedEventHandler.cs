using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public class BookingCompletedEventHandler : INotificationHandler<BookingCompletedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;

        public BookingCompletedEventHandler(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
        }

        public async Task Handle(BookingCompletedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var tutorDisplayName = NotificationNameHelper.FormatUserDisplayName(notificationEvent.TutorName, "Gia sư");
            var title = "Buổi học đã hoàn thành!";
            var message = $"{tutorDisplayName} đã hoàn thành buổi học vào {notificationEvent.Booking.ScheduledStartAt:dd/MM/yyyy HH:mm}. Hãy vào Lịch Học để đánh giá chất lượng buổi học nhé!";
            
            var notification = new Notification(
                notificationEvent.Booking.StudentId,
                title,
                message,
                NotificationType.BookingCompleted,
                notificationEvent.Booking.Id,
                "Booking"
            );

            await _notificationRepository.AddAsync(notification);

            var dto = new NotificationDto
            {
                Id = notification.Id,
                Title = title,
                Message = message,
                Type = notification.Type.ToString(),
                IsRead = false,
                CreatedAt = notification.CreatedAt
            };

            await _notificationSender.SendNotificationAsync(notification.UserId, dto);
        }
    }
}
