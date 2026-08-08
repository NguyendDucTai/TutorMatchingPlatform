using System;
using System.Collections.Generic;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Domain.Entities
{
    public class LearningGoal : BaseEntity
    {
        public Guid TutorId { get; private set; }
        public Guid StudentId { get; private set; }
        public Guid SubjectId { get; private set; }
        public string Title { get; private set; }
        public string? Description { get; private set; }
        public DateTime? TargetDate { get; private set; }
        public GoalStatus Status { get; private set; }

        private readonly List<GoalProgress> _goalProgresses = new();
        public IReadOnlyCollection<GoalProgress> GoalProgresses => _goalProgresses.AsReadOnly();

        private LearningGoal() { } // EF Core

        public LearningGoal(Guid tutorId, Guid studentId, Guid subjectId, string title, string? description, DateTime? targetDate)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.");
            
            TutorId = tutorId;
            StudentId = studentId;
            SubjectId = subjectId;
            Title = title;
            Description = description;
            TargetDate = targetDate;
            Status = GoalStatus.NotStarted;
        }

        public void UpdateDetails(string title, string? description, DateTime? targetDate)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.");
            
            Title = title;
            Description = description;
            TargetDate = targetDate;
            MarkUpdated();
        }

        public void AddProgress(int percentage, string? notes)
        {
            if (percentage < 0 || percentage > 100) throw new ArgumentException("Percentage must be between 0 and 100.");
            
            var progress = new GoalProgress(Id, percentage, notes);
            _goalProgresses.Add(progress);

            if (percentage == 100)
                Status = GoalStatus.Completed;
            else if (Status == GoalStatus.NotStarted && percentage > 0)
                Status = GoalStatus.InProgress;

            MarkUpdated();
        }

        public void CheckOverdue()
        {
            if (Status != GoalStatus.Completed && TargetDate.HasValue && TargetDate.Value < DateTime.UtcNow.Date)
            {
                Status = GoalStatus.Overdue;
                MarkUpdated();
            }
        }
    }
}
