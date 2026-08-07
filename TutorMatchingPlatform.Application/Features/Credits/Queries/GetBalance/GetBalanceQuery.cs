using System;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Credits;

namespace TutorMatchingPlatform.Application.Features.Credits.Queries.GetBalance
{
    public class GetBalanceQuery : IRequest<WalletBalanceDto>
    {
        public Guid UserId { get; set; }

        public GetBalanceQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
