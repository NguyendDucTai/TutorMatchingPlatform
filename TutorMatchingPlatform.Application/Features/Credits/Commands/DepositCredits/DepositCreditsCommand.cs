using System;
using MediatR;

namespace TutorMatchingPlatform.Application.Features.Credits.Commands.DepositCredits
{
    public class DepositCreditsCommand : IRequest<decimal>
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
