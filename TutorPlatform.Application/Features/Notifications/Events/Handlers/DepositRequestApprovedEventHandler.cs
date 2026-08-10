using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public class DepositRequestApprovedEventHandler : INotificationHandler<DepositRequestApprovedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;

        public DepositRequestApprovedEventHandler(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
        }

        public async Task Handle(DepositRequestApprovedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var title = "Nạp tiền thành công!";
            var message = $"Yêu cầu nạp tiền của bạn đã được phê duyệt. Số dư ví: {notificationEvent.NewBalance:N0} Credits (💎 +{notificationEvent.Amount:N0}).";

            var notification = new Notification(
                notificationEvent.UserId,
                title,
                message,
                NotificationType.CreditChanged,
                null,
                "Credit"
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
