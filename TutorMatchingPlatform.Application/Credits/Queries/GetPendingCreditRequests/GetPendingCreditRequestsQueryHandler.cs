using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Credits.Queries.GetPendingCreditRequests
{
    public class GetPendingCreditRequestsQueryHandler : IRequestHandler<GetPendingCreditRequestsQuery, List<CreditRequestDto>>
    {
        private readonly IAppDbContext _context;

        public GetPendingCreditRequestsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CreditRequestDto>> Handle(GetPendingCreditRequestsQuery request, CancellationToken cancellationToken)
        {
            var pendingRequests = await _context.CreditRequests
                .Include(cr => cr.User)
                .Where(cr => cr.Status == CreditRequestStatus.Pending)
                .OrderBy(cr => cr.CreatedAt)
                .Select(cr => new CreditRequestDto
                {
                    Id = cr.Id,
                    UserId = cr.UserId,
                    FullName = cr.User.FullName,
                    Email = cr.User.Email,
                    Amount = cr.Amount,
                    CreatedAt = cr.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return pendingRequests;
        }
    }
}
