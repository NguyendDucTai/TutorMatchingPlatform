using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Contracts.Notifications;
using TutorMatchingPlatform.API.Hubs;

namespace TutorMatchingPlatform.API.Services
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
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }
    }
}
