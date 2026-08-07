using System;

namespace TutorMatchingPlatform.Domain.Common
{
    public class AdminUserResult
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; } // 1 = Tutor, 2 = Student
        public bool IsActive { get; set; }
        public decimal CreditBalance { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AdminNote { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class AdminReviewResult
    {
        public Guid Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string RevieweeName { get; set; } = string.Empty; // Tutor or Student name
        public string SubjectName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int ReviewType { get; set; } // 0 = StudentToTutor, 1 = TutorToStudent
        public DateTime CreatedAt { get; set; }
    }
}
