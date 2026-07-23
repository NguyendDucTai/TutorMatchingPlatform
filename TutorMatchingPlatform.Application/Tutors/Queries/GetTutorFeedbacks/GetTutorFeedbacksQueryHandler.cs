using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Tutors.DTOs;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Tutors.Queries.GetTutorFeedbacks
{
    public class GetTutorFeedbacksQueryHandler : IRequestHandler<GetTutorFeedbacksQuery, PaginatedTutorFeedbacksDto>
    {
        private readonly IAppDbContext _context;

        public GetTutorFeedbacksQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedTutorFeedbacksDto> Handle(GetTutorFeedbacksQuery request, CancellationToken cancellationToken)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.TutorId && u.Role == UserRole.Tutor, cancellationToken);

            if (!userExists)
            {
                return null!;
            }

            var query = _context.Feedbacks
                .Include(f => f.Sender)
                .Where(f => f.ReceiverId == request.TutorId)
                .OrderByDescending(f => f.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var feedbacks = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => new TutorFeedbackDto
                {
                    FeedbackId = f.Id,
                    SessionId = f.SessionId,
                    AuthorName = f.Sender.FullName,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PaginatedTutorFeedbacksDto
            {
                TotalCount = totalCount,
                Feedbacks = feedbacks
            };
        }
    }
}
