using System;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.Domain.Entities
{
    public class Subject : BaseEntity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public string? Category { get; private set; }
        public bool IsActive { get; private set; }

        private Subject() { } // EF Core

        public Subject(string name, string? description, string? category)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Subject name cannot be empty.");
            
            Name = name;
            Description = description;
            Category = category;
            IsActive = true;
        }

        public void UpdateDetails(string name, string? description, string? category)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Subject name cannot be empty.");
            
            Name = name;
            Description = description;
            Category = category;
            MarkUpdated();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkUpdated();
        }
        
        public void Activate()
        {
            IsActive = true;
            MarkUpdated();
        }
    }
}
