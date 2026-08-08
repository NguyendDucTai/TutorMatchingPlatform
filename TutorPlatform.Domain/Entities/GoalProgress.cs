using System;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.Domain.Entities
{
    public class GoalProgress : BaseEntity
    {
        public Guid LearningGoalId { get; private set; }
        public DateTime RecordedAt { get; private set; }
        public int ProgressPercentage { get; private set; }
        public string? Notes { get; private set; }

        private GoalProgress() { } // EF Core

        public GoalProgress(Guid learningGoalId, int progressPercentage, string? notes)
        {
            LearningGoalId = learningGoalId;
            RecordedAt = DateTime.UtcNow;
            ProgressPercentage = progressPercentage;
            Notes = notes;
        }
    }
}
