using System;
using MediatR;

namespace TutorPlatform.Application.Features.Progress.Commands.CreateLearningGoal
{
    public class CreateLearningGoalCommand : IRequest<Guid>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid TutorId { get; set; }
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? TargetDate { get; set; }
    }
}
