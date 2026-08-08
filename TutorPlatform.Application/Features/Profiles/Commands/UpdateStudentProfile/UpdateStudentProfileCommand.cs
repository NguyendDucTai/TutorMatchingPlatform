using System;
using MediatR;

namespace TutorPlatform.Application.Features.Profiles.Commands.UpdateStudentProfile
{
    public class UpdateStudentProfileCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string? GradeLevel { get; set; }
        public string? LearningPreferences { get; set; }
    }
}
