using System;

namespace TutorPlatform.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        protected BaseEntity(Guid id, DateTime createdAt, DateTime? updatedAt)
        {
            Id = id;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        protected void MarkUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
