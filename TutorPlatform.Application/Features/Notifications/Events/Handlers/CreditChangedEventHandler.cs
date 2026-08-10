using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public class CreditChangedEventHandler : INotificationHandler<CreditChangedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;

        public CreditChangedEventHandler(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
        }

        public async Task Handle(CreditChangedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var title = "Biến động số dư";
            var actionStr = notificationEvent.Amount > 0 ? "nạp" : "trừ";
            var message = $"Tài khoản của bạn vừa được {actionStr} {Math.Abs(notificationEvent.Amount):N0} Credits. Số dư mới: {notificationEvent.NewBalance:N0} Credits.";
            
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
