using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Contracts.Notifications;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Notifications.Events
{
    public class NotificationEventHandlers :
        INotificationHandler<BookingCreatedEvent>,
        INotificationHandler<BookingCancelledEvent>,
        INotificationHandler<ReviewReceivedEvent>,
        INotificationHandler<CreditChangedEvent>,
        INotificationHandler<BookingConfirmedEvent>,
        INotificationHandler<DepositRequestCreatedEvent>,
        INotificationHandler<DepositRequestApprovedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSender _notificationSender;
        private readonly IUserRepository _userRepository;

        public NotificationEventHandlers(
            INotificationRepository notificationRepository,
            INotificationSender notificationSender,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _notificationSender = notificationSender;
            _userRepository = userRepository;
        }

        public async Task Handle(BookingCreatedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var title = "Lịch học mới!";
            var message = $"Học viên {notificationEvent.StudentName} vừa đặt lịch học với bạn vào {notificationEvent.Booking.ScheduledStartAt.ToString("dd/MM/yyyy HH:mm")}.";
            
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

        public async Task Handle(BookingCancelledEvent notificationEvent, CancellationToken cancellationToken)
        {
            var isStudentCancelled = notificationEvent.CancelledByUserId == notificationEvent.Booking.StudentId;
            var targetUserId = isStudentCancelled ? notificationEvent.Booking.TutorId : notificationEvent.Booking.StudentId;
            
            var title = "Lịch học bị hủy";
            var message = $"{notificationEvent.CancelledByUserName} đã hủy lịch học vào {notificationEvent.Booking.ScheduledStartAt.ToString("dd/MM/yyyy HH:mm")}.";
            
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

        public async Task Handle(ReviewReceivedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var title = "Đánh giá mới!";
            var message = $"Học viên {notificationEvent.ReviewerName} vừa để lại đánh giá {notificationEvent.Review.Rating} sao cho buổi học của bạn.";
            
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

        public async Task Handle(BookingConfirmedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var title = "Lịch học đã được xác nhận!";
            var message = $"Gia sư {notificationEvent.TutorName} đã xác nhận lịch học của bạn vào {notificationEvent.Booking.ScheduledStartAt.ToString("dd/MM/yyyy HH:mm")}. Link lớp học: {notificationEvent.Booking.MeetingLink}";
            
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

        public async Task Handle(DepositRequestCreatedEvent notificationEvent, CancellationToken cancellationToken)
        {
            var title = "Yêu cầu nạp tiền mới!";
            var message = $"Người dùng {notificationEvent.UserName} vừa tạo yêu cầu nạp {notificationEvent.Amount:N0} Credits.";

            var admins = await _userRepository.GetUsersByRoleAsync(TutorMatchingPlatform.Domain.Enums.UserRole.Admin);
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
