using System;
using System.Collections.Generic;
using MediatR;
using TutorPlatform.Application.Contracts.Progress;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Application.Features.Progress.Queries.GetLearningGoals
{
    public class GetLearningGoalsQuery : IRequest<List<LearningGoalDto>>
    {
        public Guid? StudentId { get; set; }
        public Guid? SubjectId { get; set; }
        
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid CurrentUserId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public UserRole Role { get; set; }
    }
}
