using System;
using System.Threading.Tasks;

namespace TutorPlatform.Application.Contracts.Notifications
{
    public interface INotificationSender
    {
        Task SendNotificationAsync(Guid userId, NotificationDto notification);
    }
}
