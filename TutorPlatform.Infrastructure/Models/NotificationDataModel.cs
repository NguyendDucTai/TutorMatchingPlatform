using System;

namespace TutorPlatform.Infrastructure.Models
{
    public class NotificationDataModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int Type { get; set; }
        public bool IsRead { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDataModel User { get; set; } = null!;
    }
}
