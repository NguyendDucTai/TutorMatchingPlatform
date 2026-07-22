using System;
using MediatR;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Progress.Commands.SetLearningGoal
{
    public class SetLearningGoalCommand : IRequest<int>
    {
        public int TutorId { get; set; } // Derived from current user token
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string MilestoneName { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public MilestoneStatus Status { get; set; } = MilestoneStatus.NotStarted;
    }
}
