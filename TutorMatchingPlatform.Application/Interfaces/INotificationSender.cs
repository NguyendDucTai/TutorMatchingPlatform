using System.Threading.Tasks;

namespace TutorMatchingPlatform.Application.Interfaces
{
    public interface INotificationSender
    {
        Task SendNotificationAsync(int userId, object notification);
    }
}
