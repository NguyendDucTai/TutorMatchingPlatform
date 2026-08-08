using System;
using MediatR;
using TutorPlatform.Application.Contracts.Credits;

namespace TutorPlatform.Application.Features.Credits.Queries.GetBalance
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
