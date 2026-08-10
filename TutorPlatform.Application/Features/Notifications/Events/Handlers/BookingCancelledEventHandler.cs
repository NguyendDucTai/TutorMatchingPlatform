using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public class BookingCancelledEventHandler : INotificationHandler<BookingCancelledEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;

        public BookingCancelledEventHandler(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
        }

        public async Task Handle(BookingCancelledEvent notificationEvent, CancellationToken cancellationToken)
        {
            var isStudentCancelled = notificationEvent.CancelledByUserId == notificationEvent.Booking.StudentId;
            var targetUserId = isStudentCancelled ? notificationEvent.Booking.TutorId : notificationEvent.Booking.StudentId;
            var rolePrefix = isStudentCancelled ? "Học viên" : "Gia sư";
            var actorDisplayName = NotificationNameHelper.FormatUserDisplayName(notificationEvent.CancelledByUserName, rolePrefix);
            
            var title = "Lịch học đã bị hủy!";
            var refundNote = !isStudentCancelled ? $" Đã hoàn trả {notificationEvent.Booking.CreditAmount:N0} tín chỉ về ví của bạn." : "";
            var message = $"{actorDisplayName} đã hủy lịch học vào {notificationEvent.Booking.ScheduledStartAt:dd/MM/yyyy HH:mm}.{refundNote}";
            
            var notification = new Notification(
                targetUserId,
                title,
                message,
                NotificationType.BookingCancelled,
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

            await _notificationSender.SendNotificationAsync(targetUserId, dto);
        }
    }
}
