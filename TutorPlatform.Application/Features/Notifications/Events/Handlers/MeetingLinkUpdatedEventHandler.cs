using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public class MeetingLinkUpdatedEventHandler : INotificationHandler<MeetingLinkUpdatedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;

        public MeetingLinkUpdatedEventHandler(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
        }

        public async Task Handle(MeetingLinkUpdatedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var tutorDisplayName = NotificationNameHelper.FormatUserDisplayName(notificationEvent.TutorName, "Gia sư");
            var title = "Cập nhật link phòng học!";
            var message = $"{tutorDisplayName} vừa cập nhật link phòng học mới cho buổi học vào {notificationEvent.Booking.ScheduledStartAt:dd/MM/yyyy HH:mm}. Link lớp học: {notificationEvent.NewMeetingLink}";
            
            var notification = new Notification(
                notificationEvent.Booking.StudentId,
                title,
                message,
                NotificationType.MeetingLinkUpdated,
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
