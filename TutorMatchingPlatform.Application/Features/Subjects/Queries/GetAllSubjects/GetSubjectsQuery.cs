using System.Collections.Generic;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Subjects;

namespace TutorMatchingPlatform.Application.Features.Subjects.Queries.GetAllSubjects
{
    public class GetSubjectsQuery : IRequest<List<SubjectDto>>
    {
        public bool IncludeInactive { get; set; } = false;
    }
}
