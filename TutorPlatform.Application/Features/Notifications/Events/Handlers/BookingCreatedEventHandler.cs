using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public class BookingCreatedEventHandler : INotificationHandler<BookingCreatedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;

        public BookingCreatedEventHandler(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
        }

        public async Task Handle(BookingCreatedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var studentDisplayName = NotificationNameHelper.FormatUserDisplayName(notificationEvent.StudentName, "Học viên");
            var title = "Lịch học mới!";
            var message = $"{studentDisplayName} vừa đặt lịch học với bạn vào {notificationEvent.Booking.ScheduledStartAt:dd/MM/yyyy HH:mm}.";
            
            var notification = new Notification(
                notificationEvent.Booking.TutorId,
                title,
                message,
                NotificationType.BookingCreated,
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
