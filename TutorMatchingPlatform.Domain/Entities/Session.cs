using System;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class Session : BaseEntity
    {
        public int TutorId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? MeetingLink { get; set; }
        public SessionStatus Status { get; set; }
        
        public double? Score { get; set; }
        public string? TutorComment { get; set; }
        public int? GoalCompletionPercentage { get; set; }
        public bool ReminderSent { get; set; } = false;

        // Navigation properties
        public TutorProfile Tutor { get; set; } = null!;
        public StudentProfile Student { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
        
        // Relationship to Feedback. Usually 1-2 feedbacks per session (one from tutor, one from student)
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
