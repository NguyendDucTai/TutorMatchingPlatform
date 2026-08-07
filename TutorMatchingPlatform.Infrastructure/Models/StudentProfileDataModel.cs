using System;

namespace TutorMatchingPlatform.Infrastructure.Models
{
    public class StudentProfileDataModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? GradeLevel { get; set; }
        public string? LearningPreferences { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDataModel User { get; set; } = null!;
    }
}
