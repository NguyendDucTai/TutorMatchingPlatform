using System;
using MediatR;

namespace TutorPlatform.Application.Features.Credits.Commands.DepositCredits
{
    public class DepositCreditsCommand : IRequest<decimal>
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}
