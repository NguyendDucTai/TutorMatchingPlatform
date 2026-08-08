using System;

namespace TutorPlatform.Application.Contracts.Progress
{
    public class LearningGoalDto
    {
        public Guid Id { get; set; }
        public Guid TutorId { get; set; }
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? TargetDate { get; set; }
        public int Status { get; set; }
        public int CurrentProgress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
