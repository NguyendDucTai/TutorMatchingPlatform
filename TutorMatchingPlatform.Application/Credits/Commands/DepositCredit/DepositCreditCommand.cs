using MediatR;

namespace TutorMatchingPlatform.Application.Credits.Commands.DepositCredit
{
    public class DepositCreditCommand : IRequest<DepositCreditResult>
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }

    public class DepositCreditResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RequestId { get; set; }
    }
}
