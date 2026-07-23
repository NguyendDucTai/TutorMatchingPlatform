using MediatR;
using System;
using System.Collections.Generic;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Progress.Queries.GetLearningGoals
{
    public class GetLearningGoalsQuery : IRequest<List<LearningGoalDto>>
    {
        public int UserId { get; set; }
        public int SubjectId { get; set; }
    }

    public class LearningGoalDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string MilestoneName { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public MilestoneStatus Status { get; set; }
        public int CompletionPercentage { get; set; }
    }
}
