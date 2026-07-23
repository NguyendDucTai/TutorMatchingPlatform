using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Sessions.Queries.GetSessionById;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Sessions.Queries.GetMySessions
{
    public class GetMySessionsQueryHandler : IRequestHandler<GetMySessionsQuery, List<SessionDto>>
    {
        private readonly IAppDbContext _context;

        public GetMySessionsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SessionDto>> Handle(GetMySessionsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Sessions.AsQueryable();

            query = query.Where(s => s.Student.UserId == request.UserId || s.Tutor.UserId == request.UserId);

            if (request.FromDate.HasValue)
            {
                query = query.Where(s => s.StartTime >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(s => s.EndTime <= request.ToDate.Value);
            }

            var sessions = await query.ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;

            return sessions.Select(s => new SessionDto
            {
                Id = s.Id,
                TutorId = s.TutorId,
                StudentId = s.StudentId,
                SubjectId = s.SubjectId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Status = s.Status,
                MeetingLink = s.MeetingLink,
                CanJoin = s.Status == SessionStatus.Confirmed 
                           && now >= s.StartTime.AddMinutes(-10) 
                           && now <= s.EndTime
            }).ToList();
        }
    }
}
