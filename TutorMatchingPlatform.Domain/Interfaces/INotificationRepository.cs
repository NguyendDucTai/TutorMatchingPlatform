using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<Notification> AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task MarkAllAsReadAsync(Guid userId);
    }
}
