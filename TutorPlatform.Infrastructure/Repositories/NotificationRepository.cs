using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public NotificationRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var query = _dbContext.Notifications.Where(n => n.UserId == userId);
            
            var totalCount = await query.CountAsync();
            
            var itemsData = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = itemsData.Select(MapToDomain).ToList();

            return (items, totalCount);
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            var dataModel = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id);
            if (dataModel == null) return null;
            return MapToDomain(dataModel);
        }

        public async Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            var dataModels = await _dbContext.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            
            return dataModels.Select(MapToDomain).ToList();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _dbContext.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            var dataModel = new NotificationDataModel
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = (int)notification.Type,
                IsRead = notification.IsRead,
                RelatedEntityId = notification.RelatedEntityId,
                RelatedEntityType = notification.RelatedEntityType,
                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt
            };

            _dbContext.Notifications.Add(dataModel);
            await _dbContext.SaveChangesAsync();
            return notification;
        }

        public async Task UpdateAsync(Notification notification)
        {
            var dataModel = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
            if (dataModel != null)
            {
                dataModel.IsRead = notification.IsRead;
                dataModel.UpdatedAt = notification.UpdatedAt;
                
                _dbContext.Notifications.Update(dataModel);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _dbContext.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        private Notification MapToDomain(NotificationDataModel dataModel)
        {
            var notification = new Notification(
                dataModel.UserId,
                dataModel.Title,
                dataModel.Message,
                (NotificationType)dataModel.Type,
                dataModel.RelatedEntityId,
                dataModel.RelatedEntityType
            );

            // Reflection to set internal properties if necessary, but we can set them via private fields or constructors if allowed.
            // Since we don't have a constructor taking Id, we will use reflection or assuming the BaseEntity allows setting Id.
            var idProperty = typeof(TutorPlatform.Domain.Common.BaseEntity).GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(notification, dataModel.Id);
            }
            
            var createdProperty = typeof(TutorPlatform.Domain.Common.BaseEntity).GetProperty("CreatedAt");
            if (createdProperty != null && createdProperty.CanWrite)
            {
                createdProperty.SetValue(notification, dataModel.CreatedAt);
            }
            
            if (dataModel.IsRead)
            {
                notification.MarkAsRead();
            }

            return notification;
        }
    }
}
