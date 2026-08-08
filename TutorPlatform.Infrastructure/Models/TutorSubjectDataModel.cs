using System;

namespace TutorPlatform.Infrastructure.Models
{
    public class TutorSubjectDataModel
    {
        public Guid Id { get; set; }
        public Guid TutorProfileId { get; set; }
        public Guid SubjectId { get; set; }
        public int ProficiencyLevel { get; set; }
        public decimal HourlyCredits { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public TutorProfileDataModel TutorProfile { get; set; } = null!;
        public SubjectDataModel Subject { get; set; } = null!;
    }
}
