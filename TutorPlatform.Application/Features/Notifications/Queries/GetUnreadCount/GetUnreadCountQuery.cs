using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Queries.GetUnreadCount
{
    public class GetUnreadCountQuery : IRequest<int>
    {
        public Guid UserId { get; set; }
    }

    public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
    {
        private readonly INotificationRepository _notificationRepository;

        public GetUnreadCountQueryHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
        {
            return await _notificationRepository.GetUnreadCountAsync(request.UserId);
        }
    }
}
