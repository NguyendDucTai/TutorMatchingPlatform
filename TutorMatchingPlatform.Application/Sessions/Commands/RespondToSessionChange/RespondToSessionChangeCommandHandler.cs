using System;
using System.Threading;
using TutorMatchingPlatform.Domain.Entities;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Sessions.Commands.RespondToSessionChange
{
    public class RespondToSessionChangeCommandHandler : IRequestHandler<RespondToSessionChangeCommand, RespondToSessionChangeResult>
    {
        private readonly IAppDbContext _context;

        public RespondToSessionChangeCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<RespondToSessionChangeResult> Handle(RespondToSessionChangeCommand request, CancellationToken cancellationToken)
        {
            var changeRequest = await _context.SessionChangeRequests
                .Include(cr => cr.Session).ThenInclude(s => s.Tutor).ThenInclude(t => t.User)
                .Include(cr => cr.Session).ThenInclude(s => s.Student).ThenInclude(st => st.User)
                .SingleOrDefaultAsync(cr => cr.Id == request.ChangeRequestId, cancellationToken);

            if (changeRequest == null)
            {
                return new RespondToSessionChangeResult { Success = false, Message = "Request not found." };
            }

            if (changeRequest.Status != SessionChangeRequestStatus.Pending)
            {
                return new RespondToSessionChangeResult { Success = false, Message = "Request is already processed." };
            }

            var session = changeRequest.Session;

            // The responder must be the OTHER party (not the requester)
            int otherPartyId = changeRequest.RequesterId == session.Tutor.User.Id 
                ? session.Student.User.Id 
                : session.Tutor.User.Id;

            if (request.ResponderUserId != otherPartyId)
            {
                return new RespondToSessionChangeResult { Success = false, Message = "Unauthorized: Only the other party can respond." };
            }

            if (request.Accept)
            {
                changeRequest.Status = SessionChangeRequestStatus.Accepted;

                if (changeRequest.ChangeType == SessionChangeType.Cancel)
                {
                    session.Status = SessionStatus.Cancelled;

                    // Refund SessionFee to Student
                    var originalTransaction = await _context.CreditTransactions
                        .FirstOrDefaultAsync(ct => ct.ReferenceId == session.Id.ToString() && ct.Type == CreditTransactionType.SessionFee, cancellationToken);

                    if (originalTransaction != null)
                    {
                        decimal refundAmount = Math.Abs(originalTransaction.Amount);
                        session.Student.User.CreditBalance += refundAmount;

                        _context.CreditTransactions.Add(new CreditTransaction
                        {
                            UserId = session.Student.User.Id,
                            Amount = refundAmount,
                            Type = CreditTransactionType.Refund,
                            Description = "Refund for cancelled session",
                            ReferenceId = session.Id.ToString(),
                            CreatedAt = System.DateTime.UtcNow
                        });
                    }
                }
                else if (changeRequest.ChangeType == SessionChangeType.Reschedule)
                {
                    session.StartTime = changeRequest.ProposedStartTime!.Value;
                    session.EndTime = changeRequest.ProposedEndTime!.Value;
                    session.Status = SessionStatus.Confirmed;
                }
            }
            else // Decline
            {
                changeRequest.Status = SessionChangeRequestStatus.Declined;
                
                // Revert session back to Confirmed
                session.Status = SessionStatus.Confirmed;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new RespondToSessionChangeResult
            {
                Success = true,
                Message = request.Accept ? "Request accepted." : "Request declined."
            };
        }
    }
}
