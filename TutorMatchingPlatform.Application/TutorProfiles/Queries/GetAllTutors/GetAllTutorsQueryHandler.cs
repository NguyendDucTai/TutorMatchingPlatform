using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.TutorProfiles.DTOs;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.TutorProfiles.Queries.GetAllTutors
{
    public class GetAllTutorsQueryHandler : IRequestHandler<GetAllTutorsQuery, GetAllTutorsResult>
    {
        private readonly IAppDbContext _context;

        public GetAllTutorsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<GetAllTutorsResult> Handle(GetAllTutorsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users
                .Include(u => u.TutorProfile)
                .Where(u => u.Role == UserRole.Tutor && u.TutorProfile != null)
                .OrderByDescending(u => u.TutorProfile!.UpdatedAt ?? u.TutorProfile.CreatedAt);

            var count = await query.CountAsync(cancellationToken);
            var items = await query.Skip((request.PageNumber - 1) * request.PageSize)
                                   .Take(request.PageSize)
                                   .Select(u => new TutorProfileDto
                                   {
                                       Id = u.TutorProfile!.Id,
                                       UserId = u.Id,
                                       FullName = u.FullName,
                                       SubjectsJson = u.TutorProfile.SubjectsJson,
                                       Qualifications = u.TutorProfile.Qualifications,
                                       SubmissionDate = u.TutorProfile.UpdatedAt ?? u.TutorProfile.CreatedAt,
                                       Status = u.TutorProfile.Status.ToString()
                                   })
                                   .ToListAsync(cancellationToken);

            return new GetAllTutorsResult
            {
                Tutors = items,
                TotalCount = count,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
