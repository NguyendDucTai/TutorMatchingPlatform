using System;
using MediatR;

namespace TutorPlatform.Application.Features.Progress.Commands.RecordGoalProgress
{
    public class RecordGoalProgressCommand : IRequest<bool>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid GoalId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid TutorId { get; set; }
        public int ProgressPercentage { get; set; }
        public string? Notes { get; set; }
    }
}
