using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Credits.Commands.DepositCredit
{
    public class DepositCreditCommandHandler : IRequestHandler<DepositCreditCommand, DepositCreditResult>
    {
        private readonly IAppDbContext _context;

        public DepositCreditCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<DepositCreditResult> Handle(DepositCreditCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null)
            {
                return new DepositCreditResult { Success = false, Message = "User not found." };
            }

            var creditRequest = new CreditRequest
            {
                UserId = request.UserId,
                Amount = request.Amount,
                Status = CreditRequestStatus.Pending,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow
            };

            _context.CreditRequests.Add(creditRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return new DepositCreditResult
            {
                Success = true,
                Message = "Deposit request submitted successfully. Pending Admin approval.",
                RequestId = creditRequest.Id
            };
        }
    }
}
