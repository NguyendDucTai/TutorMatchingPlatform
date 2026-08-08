using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Commands.MarkAllAsRead
{
    public class MarkAllAsReadCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
    }

    public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, bool>
    {
        private readonly INotificationRepository _notificationRepository;

        public MarkAllAsReadCommandHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
        {
            await _notificationRepository.MarkAllAsReadAsync(request.UserId);
            return true;
        }
    }
}
