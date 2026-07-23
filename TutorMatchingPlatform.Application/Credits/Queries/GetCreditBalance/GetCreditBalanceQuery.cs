using MediatR;

namespace TutorMatchingPlatform.Application.Credits.Queries.GetCreditBalance
{
    public class GetCreditBalanceQuery : IRequest<decimal>
    {
        public int UserId { get; set; }
    }
}
