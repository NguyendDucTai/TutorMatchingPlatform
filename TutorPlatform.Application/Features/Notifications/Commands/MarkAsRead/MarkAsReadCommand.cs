using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Commands.MarkAsRead
{
    public class MarkAsReadCommand : IRequest<bool>
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
    }

    public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, bool>
    {
        private readonly INotificationRepository _notificationRepository;

        public MarkAsReadCommandHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
            
            if (notification == null)
            {
                throw new NotFoundException("Notification", request.NotificationId);
            }

            if (notification.UserId != request.UserId)
            {
                throw new ForbiddenException("You do not have permission to mark this notification as read.");
            }

            if (!notification.IsRead)
            {
                notification.MarkAsRead();
                await _notificationRepository.UpdateAsync(notification);
            }

            return true;
        }
    }
}
