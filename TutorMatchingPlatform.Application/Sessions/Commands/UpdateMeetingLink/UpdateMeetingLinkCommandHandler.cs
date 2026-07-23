using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Sessions.Commands.UpdateMeetingLink
{
    public class UpdateMeetingLinkCommandHandler : IRequestHandler<UpdateMeetingLinkCommand, UpdateMeetingLinkResult>
    {
        private readonly IAppDbContext _context;

        public UpdateMeetingLinkCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<UpdateMeetingLinkResult> Handle(UpdateMeetingLinkCommand request, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(s => s.Tutor)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
            {
                return new UpdateMeetingLinkResult { Success = false, Message = "Session not found." };
            }

            if (session.Tutor.User.Id != request.TutorUserId)
            {
                return new UpdateMeetingLinkResult { Success = false, Message = "Only the assigned tutor can update the meeting link." };
            }

            if (string.IsNullOrWhiteSpace(request.MeetingLink))
            {
                return new UpdateMeetingLinkResult { Success = false, Message = "Meeting link cannot be empty." };
            }

            session.MeetingLink = request.MeetingLink;
            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateMeetingLinkResult { Success = true, Message = "Meeting link updated successfully." };
        }
    }
}
