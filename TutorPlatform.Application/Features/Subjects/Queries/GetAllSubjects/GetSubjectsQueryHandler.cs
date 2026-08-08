using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Contracts.Subjects;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Subjects.Queries.GetAllSubjects
{
    public class GetSubjectsQueryHandler : IRequestHandler<GetSubjectsQuery, List<SubjectDto>>
    {
        private readonly ISubjectRepository _subjectRepository;

        public GetSubjectsQueryHandler(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<List<SubjectDto>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
        {
            var subjects = request.IncludeInactive 
                ? await _subjectRepository.GetAllAsync() 
                : await _subjectRepository.GetAllActiveAsync();
            
            return subjects.Select(s => new SubjectDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive
            }).ToList();
        }
    }
}
