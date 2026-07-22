using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Feedbacks.Commands.RateSession
{
    public class RateSessionCommandHandler : IRequestHandler<RateSessionCommand, RateSessionResult>
    {
        private readonly IAppDbContext _context;

        public RateSessionCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<RateSessionResult> Handle(RateSessionCommand request, CancellationToken cancellationToken)
        {
            // 1. Fetch Session
            var session = await _context.Sessions
                .Include(s => s.Tutor).ThenInclude(t => t.User)
                .Include(s => s.Student).ThenInclude(st => st.User)
                .SingleOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
            {
                return new RateSessionResult { Success = false, Message = "Session not found." };
            }

            // 2. Validate Session Status (BR-05)
            if (session.Status != SessionStatus.Completed)
            {
                // Must be Completed to rate
                return new RateSessionResult { Success = false, Message = "MSG08" };
            }

            // 3. Determine roles and Receiver
            int receiverId = 0;
            if (request.SenderUserId == session.Student.User.Id)
            {
                receiverId = session.Tutor.User.Id;
            }
            else if (request.SenderUserId == session.Tutor.User.Id)
            {
                receiverId = session.Student.User.Id;
            }
            else
            {
                return new RateSessionResult { Success = false, Message = "Unauthorized: You are not a participant in this session." };
            }

            // 4. Check for duplicate ratings (BR-06)
            var existingFeedback = await _context.Feedbacks
                .AnyAsync(f => f.SessionId == request.SessionId && f.SenderId == request.SenderUserId, cancellationToken);

            if (existingFeedback)
            {
                // Already rated this session
                return new RateSessionResult { Success = false, Message = "You have already rated this session." };
            }

            // 5. Create Feedback
            var feedback = new Feedback
            {
                SessionId = request.SessionId,
                SenderId = request.SenderUserId,
                ReceiverId = receiverId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync(cancellationToken);

            return new RateSessionResult
            {
                Success = true,
                Message = "Rating submitted successfully.",
                FeedbackId = feedback.Id
            };
        }
    }
}
