using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using TutorPlatform.Application.Contracts.Notifications;
using TutorPlatform.API.Hubs;

namespace TutorPlatform.API.Services
{
    public class NotificationSender : INotificationSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationSender(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(Guid userId, NotificationDto notification)
        {
            var userGroupId = userId.ToString().ToLowerInvariant();
            await _hubContext.Clients.Group(userGroupId).SendAsync("ReceiveNotification", notification);
        }
    }
}
