using System.Collections.Generic;
using MediatR;
using TutorMatchingPlatform.Application.Subjects.DTOs;

namespace TutorMatchingPlatform.Application.Subjects.Queries.GetAllSubjects
{
    public class GetAllSubjectsQuery : IRequest<List<SubjectDto>>
    {
        public bool OnlyActive { get; set; } = true;
    }
}
