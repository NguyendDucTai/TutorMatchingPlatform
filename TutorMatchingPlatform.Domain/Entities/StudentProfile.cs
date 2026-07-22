using System.Collections.Generic;
using TutorMatchingPlatform.Domain.Common;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class StudentProfile : BaseEntity
    {
        public int UserId { get; set; }
        public string? StudyGoals { get; set; }
        public string? TargetSubjectsJson { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Feedback> FeedbacksReceived { get; set; } = new List<Feedback>();
        public ICollection<LearningMilestone> LearningMilestones { get; set; } = new List<LearningMilestone>();
    }
}
