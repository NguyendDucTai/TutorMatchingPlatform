using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public class DepositRequestCreatedEventHandler : INotificationHandler<DepositRequestCreatedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;
        private readonly IUserRepository _userRepository;

        public DepositRequestCreatedEventHandler(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
            _userRepository = userRepository;
        }

        public async Task Handle(DepositRequestCreatedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var title = "Yêu cầu nạp tiền mới!";
            var message = $"Người dùng {notificationEvent.UserName} vừa tạo yêu cầu nạp {notificationEvent.Amount:N0} Credits.";

            var admins = await _userRepository.GetUsersByRoleAsync(UserRole.Admin);
            foreach (var admin in admins)
            {
                var notification = new Notification(
                    admin.Id,
                    title,
                    message,
                    NotificationType.System,
                    notificationEvent.RequestId,
                    "DepositRequest"
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
}
