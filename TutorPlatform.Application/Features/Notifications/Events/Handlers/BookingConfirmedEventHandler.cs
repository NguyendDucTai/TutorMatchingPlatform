using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public class BookingConfirmedEventHandler : INotificationHandler<BookingConfirmedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;

        public BookingConfirmedEventHandler(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
        }

        public async Task Handle(BookingConfirmedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var tutorDisplayName = NotificationNameHelper.FormatUserDisplayName(notificationEvent.TutorName, "Gia sư");
            var title = "Lịch học đã được xác nhận!";
            var message = $"{tutorDisplayName} đã xác nhận lịch học của bạn vào {notificationEvent.Booking.ScheduledStartAt:dd/MM/yyyy HH:mm}. Link lớp học: {notificationEvent.Booking.MeetingLink}";
            
            var notification = new Notification(
                notificationEvent.Booking.StudentId,
                title,
                message,
                NotificationType.BookingConfirmed,
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
