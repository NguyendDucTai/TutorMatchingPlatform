using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public class ReviewReceivedEventHandler : INotificationHandler<ReviewReceivedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;

        public ReviewReceivedEventHandler(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
        }

        public async Task Handle(ReviewReceivedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var reviewerDisplayName = NotificationNameHelper.FormatUserDisplayName(notificationEvent.ReviewerName, "Học viên");
            var title = "Đánh giá mới!";
            var message = $"{reviewerDisplayName} vừa để lại đánh giá {notificationEvent.Review.Rating} sao cho buổi học của bạn.";
            
            var notification = new Notification(
                notificationEvent.Review.RevieweeId,
                title,
                message,
                NotificationType.ReviewReceived,
                notificationEvent.Review.Id,
                "Review"
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
