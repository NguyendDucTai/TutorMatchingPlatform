using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Tutors.DTOs;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Tutors.Queries.GetTutorDetails
{
    public class GetTutorDetailsQueryHandler : IRequestHandler<GetTutorDetailsQuery, TutorDetailsDto>
    {
        private readonly IAppDbContext _context;

        public GetTutorDetailsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<TutorDetailsDto> Handle(GetTutorDetailsQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.TutorProfile)
                .SingleOrDefaultAsync(u => u.Id == request.TutorId && u.Role == UserRole.Tutor, cancellationToken);

            if (user == null || user.TutorProfile == null)
            {
                return null!;
            }

            var feedbacks = await _context.Feedbacks
                .Where(f => f.ReceiverId == user.Id)
                .ToListAsync(cancellationToken);

            // Calculate average rating
            double averageRating = 0;
            if (feedbacks.Any())
            {
                averageRating = feedbacks.Average(f => f.Rating);
            }

            return new TutorDetailsDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Bio = user.TutorProfile.Bio,
                Qualifications = user.TutorProfile.Qualifications,
                SubjectsJson = user.TutorProfile.SubjectsJson,
                FreeSchedulesJson = user.TutorProfile.FreeSchedulesJson,
                AverageRating = averageRating,
                TotalReviews = feedbacks.Count
            };
        }
    }
}
