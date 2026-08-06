using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Infrastructure.Services
{
    public class SignalRNotificationSender<THub> : INotificationSender where THub : Hub
    {
        private readonly IHubContext<THub> _hubContext;

        public SignalRNotificationSender(IHubContext<THub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(int userId, object notification)
        {
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }
    }
}
