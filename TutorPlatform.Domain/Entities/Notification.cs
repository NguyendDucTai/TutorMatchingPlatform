using System;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public NotificationType Type { get; private set; }
        public bool IsRead { get; private set; }
        public Guid? RelatedEntityId { get; private set; }
        public string? RelatedEntityType { get; private set; }

        private Notification() { } // EF Core

        public Notification(Guid userId, string title, string message, NotificationType type, Guid? relatedEntityId = null, string? relatedEntityType = null)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.");
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be empty.");

            UserId = userId;
            Title = title;
            Message = message;
            Type = type;
            IsRead = false;
            RelatedEntityId = relatedEntityId;
            RelatedEntityType = relatedEntityType;
        }

        public void MarkAsRead()
        {
            IsRead = true;
            MarkUpdated();
        }
    }
}
