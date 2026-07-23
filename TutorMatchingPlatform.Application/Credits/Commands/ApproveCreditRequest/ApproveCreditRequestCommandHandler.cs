using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Credits.Commands.ApproveCreditRequest
{
    public class ApproveCreditRequestCommandHandler : IRequestHandler<ApproveCreditRequestCommand, ApproveCreditRequestResult>
    {
        private readonly IAppDbContext _context;

        public ApproveCreditRequestCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ApproveCreditRequestResult> Handle(ApproveCreditRequestCommand request, CancellationToken cancellationToken)
        {
            var creditRequest = await _context.CreditRequests
                .Include(cr => cr.User)
                .SingleOrDefaultAsync(cr => cr.Id == request.CreditRequestId, cancellationToken);

            if (creditRequest == null)
            {
                return new ApproveCreditRequestResult { Success = false, Message = "Credit request not found." };
            }

            if (creditRequest.Status != CreditRequestStatus.Pending)
            {
                return new ApproveCreditRequestResult { Success = false, Message = "Only pending requests can be approved." };
            }

            // Update request
            creditRequest.Status = CreditRequestStatus.Approved;
            creditRequest.ProcessedAt = DateTime.UtcNow;
            creditRequest.ProcessedBy = request.AdminUserId;

            // Add credit balance
            creditRequest.User.CreditBalance += creditRequest.Amount;

            // Create Transaction
            var transaction = new CreditTransaction
            {
                UserId = creditRequest.UserId,
                Amount = creditRequest.Amount,
                Type = CreditTransactionType.Deposit,
                Description = "Credit deposit approved by Admin",
                ReferenceId = creditRequest.Id.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            _context.CreditTransactions.Add(transaction);

            await _context.SaveChangesAsync(cancellationToken);

            return new ApproveCreditRequestResult { Success = true, Message = "Credit request approved successfully." };
        }
    }
}
