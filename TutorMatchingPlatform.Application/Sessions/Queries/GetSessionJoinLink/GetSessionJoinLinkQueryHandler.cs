using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Sessions.Queries.GetSessionJoinLink
{
    public class GetSessionJoinLinkQueryHandler : IRequestHandler<GetSessionJoinLinkQuery, SessionJoinLinkResult>
    {
        private readonly IAppDbContext _context;

        public GetSessionJoinLinkQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<SessionJoinLinkResult> Handle(GetSessionJoinLinkQuery request, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(s => s.Tutor).ThenInclude(t => t.User)
                .Include(s => s.Student).ThenInclude(st => st.User)
                .SingleOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
            {
                return new SessionJoinLinkResult { Success = false, Message = "Session not found." };
            }

            if (session.Status == SessionStatus.Cancelled)
            {
                return new SessionJoinLinkResult { Success = false, Message = "Session is cancelled." };
            }

            bool isTutor = session.Tutor.User.Id == request.UserId;
            bool isStudent = session.Student.User.Id == request.UserId;

            if (!isTutor && !isStudent)
            {
                return new SessionJoinLinkResult { Success = false, Message = "Unauthorized: You are not a participant in this session." };
            }

            var result = new SessionJoinLinkResult
            {
                Success = true,
                MeetingLink = session.MeetingLink
            };

            // Join control enabled from 10 min before start until scheduled end
            var now = DateTime.UtcNow;
            if (now >= session.StartTime.AddMinutes(-10) && now <= session.EndTime)
            {
                result.CanJoin = true;
            }
            else
            {
                result.CanJoin = false;
                if (now < session.StartTime.AddMinutes(-10))
                {
                    result.Message = "Meeting is not currently active. You can join 10 minutes before the start time.";
                }
                else
                {
                    result.Message = "Meeting has already ended.";
                }
            }

            // Missing link shows a warning and prompts the Tutor to add one
            if (string.IsNullOrEmpty(session.MeetingLink))
            {
                result.CanJoin = false;
                result.Message = "Meeting link is missing. (MSG08)";
                
                if (isTutor)
                {
                    result.PromptTutorToAddLink = true;
                }
            }

            return result;
        }
    }
}
