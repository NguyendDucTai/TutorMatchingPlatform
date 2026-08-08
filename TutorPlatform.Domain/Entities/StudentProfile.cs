using System;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.Domain.Entities
{
    public class StudentProfile : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string? GradeLevel { get; private set; }
        public string? LearningPreferences { get; private set; }
        public decimal AverageRating { get; private set; }

        private StudentProfile() { } // EF Core

        public StudentProfile(Guid userId, string? gradeLevel, string? learningPreferences)
        {
            UserId = userId;
            GradeLevel = gradeLevel;
            LearningPreferences = learningPreferences;
            AverageRating = 0;
        }

        public void UpdateDetails(string? gradeLevel, string? learningPreferences)
        {
            GradeLevel = gradeLevel;
            LearningPreferences = learningPreferences;
            MarkUpdated();
        }

        // Student's rating calculated based on reviews from tutors
        public void RecalculateRating(int newRating, int currentTotalReviews)
        {
            decimal totalScore = (AverageRating * currentTotalReviews) + newRating;
            AverageRating = totalScore / (currentTotalReviews + 1);
            MarkUpdated();
        }
    }
}
