using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Credits.Queries.GetCreditTransactions
{
    public class GetCreditTransactionsQueryHandler : IRequestHandler<GetCreditTransactionsQuery, List<CreditTransactionDto>>
    {
        private readonly IAppDbContext _context;

        public GetCreditTransactionsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CreditTransactionDto>> Handle(GetCreditTransactionsQuery request, CancellationToken cancellationToken)
        {
            var transactions = await _context.CreditTransactions
                .AsNoTracking()
                .Where(ct => ct.UserId == request.UserId)
                .OrderByDescending(ct => ct.CreatedAt)
                .Select(ct => new CreditTransactionDto
                {
                    Id = ct.Id,
                    Amount = ct.Amount,
                    Type = ct.Type.ToString(),
                    ReferenceId = ct.ReferenceId,
                    Description = ct.Description,
                    CreatedAt = ct.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return transactions;
        }
    }
}
