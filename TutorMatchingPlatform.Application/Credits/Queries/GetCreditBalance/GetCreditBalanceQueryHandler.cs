using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Credits.Queries.GetCreditBalance
{
    public class GetCreditBalanceQueryHandler : IRequestHandler<GetCreditBalanceQuery, decimal>
    {
        private readonly IAppDbContext _context;

        public GetCreditBalanceQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> Handle(GetCreditBalanceQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            return user?.CreditBalance ?? 0m;
        }
    }
}
