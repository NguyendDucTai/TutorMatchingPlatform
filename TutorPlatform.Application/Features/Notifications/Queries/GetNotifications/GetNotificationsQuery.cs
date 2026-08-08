using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Domain.Common;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Notifications.Queries.GetNotifications
{
    public class GetNotificationsQuery : IRequest<PagedResult<NotificationDto>>
    {
        public Guid UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
    {
        private readonly INotificationRepository _notificationRepository;

        public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var pagedResult = await _notificationRepository.GetPagedByUserIdAsync(request.UserId, request.PageNumber, request.PageSize);

            var items = pagedResult.Items.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType,
                CreatedAt = n.CreatedAt
            }).ToList();

            return new PagedResult<NotificationDto>(items, pagedResult.TotalCount);
        }
    }
}
