using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Sessions.Commands.ProposeSessionChange
{
    public class ProposeSessionChangeCommandHandler : IRequestHandler<ProposeSessionChangeCommand, ProposeSessionChangeResult>
    {
        private readonly IAppDbContext _context;

        public ProposeSessionChangeCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ProposeSessionChangeResult> Handle(ProposeSessionChangeCommand request, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(s => s.Tutor).ThenInclude(t => t.User)
                .Include(s => s.Student).ThenInclude(st => st.User)
                .SingleOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
            {
                return new ProposeSessionChangeResult { Success = false, Message = "Session not found." };
            }

            if (session.Status != SessionStatus.Confirmed)
            {
                return new ProposeSessionChangeResult { Success = false, Message = "Only Confirmed sessions can be changed." };
            }

            if (request.RequesterUserId != session.Tutor.User.Id && request.RequesterUserId != session.Student.User.Id)
            {
                return new ProposeSessionChangeResult { Success = false, Message = "Unauthorized: You are not a participant." };
            }

            // Check if < 24h
            bool isLate = (session.StartTime - DateTime.UtcNow).TotalHours < 24;

            // If Reschedule, check conflict
            if (request.ChangeType == SessionChangeType.Reschedule)
            {
                // Conflict detection (BR-01, BR-02)
                var hasConflict = await _context.Sessions.AnyAsync(s => 
                    s.Id != session.Id &&
                    s.Status == SessionStatus.Confirmed &&
                    (s.TutorId == session.TutorId || s.StudentId == session.StudentId) &&
                    s.StartTime < request.NewEndTime && s.EndTime > request.NewStartTime, 
                    cancellationToken);

                if (hasConflict)
                {
                    return new ProposeSessionChangeResult { Success = false, Message = "The selected time slot is already booked. Please choose another time slot." }; // Time slot already taken (conflict)
                }
            }

            // Update Session Status
            session.Status = SessionStatus.PendingChangeConfirmation;

            // Create Request
            var changeRequest = new SessionChangeRequest
            {
                SessionId = session.Id,
                RequesterId = request.RequesterUserId,
                ChangeType = request.ChangeType,
                Status = SessionChangeRequestStatus.Pending,
                ProposedStartTime = request.NewStartTime,
                ProposedEndTime = request.NewEndTime
            };

            _context.SessionChangeRequests.Add(changeRequest);
            await _context.SaveChangesAsync(cancellationToken);

            // Log or notify the other party (MSG03/MSG12 logic)
            // System sends the request to the other party...
            
            if (isLate)
            {
                // Find original session fee
                var originalTransaction = await _context.CreditTransactions
                    .FirstOrDefaultAsync(ct => ct.ReferenceId == session.Id.ToString() && ct.Type == CreditTransactionType.SessionFee, cancellationToken);
                
                decimal sessionFee = originalTransaction != null ? Math.Abs(originalTransaction.Amount) : 0;
                decimal lateFee = sessionFee * 0.3m; // 30%

                if (lateFee > 0)
                {
                    // Deduct from requester
                    var requesterUser = request.RequesterUserId == session.Student.User.Id ? session.Student.User : session.Tutor.User;
                    requesterUser.CreditBalance -= lateFee;

                    _context.CreditTransactions.Add(new CreditTransaction
                    {
                        UserId = requesterUser.Id,
                        Amount = -lateFee,
                        Type = CreditTransactionType.LateCancellationFee,
                        Description = "Late change fee (30%)",
                        ReferenceId = session.Id.ToString(),
                        CreatedAt = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            return new ProposeSessionChangeResult
            {
                Success = true,
                Message = "Change proposed successfully.",
                ChangeRequestId = changeRequest.Id,
                IsLateCancellation = isLate
            };
        }
    }
}
