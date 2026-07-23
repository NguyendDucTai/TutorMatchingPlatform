using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Credits.Commands.RejectCreditRequest
{
    public class RejectCreditRequestCommandHandler : IRequestHandler<RejectCreditRequestCommand, RejectCreditRequestResult>
    {
        private readonly IAppDbContext _context;

        public RejectCreditRequestCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<RejectCreditRequestResult> Handle(RejectCreditRequestCommand request, CancellationToken cancellationToken)
        {
            var creditRequest = await _context.CreditRequests
                .SingleOrDefaultAsync(cr => cr.Id == request.CreditRequestId, cancellationToken);

            if (creditRequest == null)
            {
                return new RejectCreditRequestResult { Success = false, Message = "Credit request not found." };
            }

            if (creditRequest.Status != CreditRequestStatus.Pending)
            {
                return new RejectCreditRequestResult { Success = false, Message = "Only pending requests can be rejected." };
            }

            // Update request
            creditRequest.Status = CreditRequestStatus.Rejected;
            creditRequest.ProcessedAt = DateTime.UtcNow;
            creditRequest.ProcessedBy = request.AdminUserId;
            creditRequest.Note = request.Reason;

            await _context.SaveChangesAsync(cancellationToken);

            return new RejectCreditRequestResult { Success = true, Message = "Credit request rejected successfully." };
        }
    }
}
