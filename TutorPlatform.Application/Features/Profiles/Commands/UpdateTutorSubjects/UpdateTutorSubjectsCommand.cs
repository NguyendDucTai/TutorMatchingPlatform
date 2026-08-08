using System;
using System.Collections.Generic;
using MediatR;

namespace TutorPlatform.Application.Features.Profiles.Commands.UpdateTutorSubjects
{
    public class UpdateTutorSubjectsCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public List<SubjectExperienceDto> Subjects { get; set; } = new List<SubjectExperienceDto>();
    }

    public class SubjectExperienceDto
    {
        public Guid SubjectId { get; set; }
        public int ProficiencyLevel { get; set; }
        public decimal HourlyCredits { get; set; }
    }
}
