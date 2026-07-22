using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Subjects.DTOs;

namespace TutorMatchingPlatform.Application.Subjects.Queries.GetAllSubjects
{
    public class GetAllSubjectsQueryHandler : IRequestHandler<GetAllSubjectsQuery, List<SubjectDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllSubjectsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubjectDto>> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Subjects.AsQueryable();

            if (request.OnlyActive)
            {
                query = query.Where(s => s.IsActive);
            }

            return await query
                .Select(s => new SubjectDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    IsActive = s.IsActive
                })
                .ToListAsync(cancellationToken);
        }
    }
}
