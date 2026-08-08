using System;
using System.Collections.Generic;

namespace TutorPlatform.Infrastructure.Models
{
    public class LearningGoalDataModel
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? TargetDate { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDataModel Tutor { get; set; } = null!;
        public UserDataModel Student { get; set; } = null!;
        public SubjectDataModel Subject { get; set; } = null!;
        public ICollection<GoalProgressDataModel> GoalProgresses { get; set; } = new List<GoalProgressDataModel>();
    }
}
