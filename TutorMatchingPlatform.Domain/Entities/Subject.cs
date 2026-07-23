using System.Collections.Generic;
using TutorMatchingPlatform.Domain.Common;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class Subject : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<LearningMilestone> LearningMilestones { get; set; } = new List<LearningMilestone>();
    }
}
