using System.Collections.Generic;
using MediatR;
using TutorPlatform.Application.Contracts.Subjects;

namespace TutorPlatform.Application.Features.Subjects.Queries.GetAllSubjects
{
    public class GetSubjectsQuery : IRequest<List<SubjectDto>>
    {
        public bool IncludeInactive { get; set; } = false;
    }
}
