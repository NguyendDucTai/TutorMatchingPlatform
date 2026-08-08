using System;

namespace TutorPlatform.Infrastructure.Models
{
    public class GoalProgressDataModel
    {
        public Guid Id { get; set; }
        public Guid LearningGoalId { get; set; }
        public DateTime RecordedAt { get; set; }
        public int ProgressPercentage { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public LearningGoalDataModel LearningGoal { get; set; } = null!;
    }
}
