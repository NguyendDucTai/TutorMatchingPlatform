using System;
using MediatR;

namespace TutorPlatform.Application.Features.Progress.Commands.UpdateLearningGoal
{
    public class UpdateLearningGoalCommand : IRequest<bool>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid GoalId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid TutorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? TargetDate { get; set; }
    }
}
