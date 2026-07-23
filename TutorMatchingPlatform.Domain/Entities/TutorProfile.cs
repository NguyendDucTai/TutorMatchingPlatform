using System.Collections.Generic;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class TutorProfile : BaseEntity
    {
        public int UserId { get; set; }
        public string? Qualifications { get; set; }
        public decimal HourlyRate { get; set; }
        public ProfileStatus Status { get; set; }
        public string? Bio { get; set; }
        public string? FreeSchedulesJson { get; set; }
        public string? SubjectsJson { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Feedback> FeedbacksReceived { get; set; } = new List<Feedback>();
    }
}
