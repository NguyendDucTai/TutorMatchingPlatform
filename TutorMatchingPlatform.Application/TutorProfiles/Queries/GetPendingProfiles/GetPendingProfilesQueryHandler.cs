using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.TutorProfiles.DTOs;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.TutorProfiles.Queries.GetPendingProfiles
{
    public class GetPendingProfilesQueryHandler : IRequestHandler<GetPendingProfilesQuery, List<TutorProfileDto>>
    {
        private readonly IAppDbContext _context;

        public GetPendingProfilesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TutorProfileDto>> Handle(GetPendingProfilesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users
                .Include(u => u.TutorProfile)
                .Where(u => u.Role == UserRole.Tutor && u.TutorProfile != null && u.TutorProfile.Status == ProfileStatus.Pending);

            var list = await query.ToListAsync(cancellationToken);

            // Filter by subject manually since SubjectsJson is a JSON string
            if (request.SubjectId.HasValue)
            {
                var searchToken = $"\"SubjectId\":{request.SubjectId.Value}";
                list = list.Where(u => u.TutorProfile!.SubjectsJson != null && u.TutorProfile.SubjectsJson.Replace(" ", "").Contains(searchToken)).ToList();
            }

            return list.Select(u => new TutorProfileDto
            {
                Id = u.TutorProfile!.Id,
                UserId = u.Id,
                FullName = u.FullName,
                SubjectsJson = u.TutorProfile.SubjectsJson,
                Qualifications = u.TutorProfile.Qualifications,
                SubmissionDate = u.TutorProfile.UpdatedAt ?? u.TutorProfile.CreatedAt,
                Status = u.TutorProfile.Status.ToString()
            }).ToList();
        }
    }
}
