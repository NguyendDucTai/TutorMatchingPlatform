using System;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class LearningMilestone : BaseEntity
    {
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string MilestoneName { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public MilestoneStatus Status { get; set; }
        public int CompletionPercentage { get; set; } = 0; // 0-100

        // Navigation properties
        public StudentProfile Student { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
    }
}
