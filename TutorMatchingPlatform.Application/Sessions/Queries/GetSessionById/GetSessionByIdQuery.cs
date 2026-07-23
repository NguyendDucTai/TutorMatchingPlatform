using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Sessions.Queries.GetSessionById
{
    public class GetSessionByIdQuery : IRequest<SessionDto?>
    {
        public int SessionId { get; set; }
    }

    public class SessionDto
    {
        public int Id { get; set; }
        public int TutorId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public SessionStatus Status { get; set; }
        public string? MeetingLink { get; set; }
        public bool CanJoin { get; set; }
    }

    public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, SessionDto?>
    {
        private readonly IAppDbContext _context;

        public GetSessionByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<SessionDto?> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null) return null;

            var now = DateTime.UtcNow;
            bool canJoin = session.Status == SessionStatus.Confirmed 
                           && now >= session.StartTime.AddMinutes(-10) 
                           && now <= session.EndTime;

            return new SessionDto
            {
                Id = session.Id,
                TutorId = session.TutorId,
                StudentId = session.StudentId,
                SubjectId = session.SubjectId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Status = session.Status,
                MeetingLink = session.MeetingLink,
                CanJoin = canJoin
            };
        }
    }
}
