using MediatR;

namespace TutorMatchingPlatform.Application.Credits.Commands.RejectCreditRequest
{
    public class RejectCreditRequestCommand : IRequest<RejectCreditRequestResult>
    {
        public int CreditRequestId { get; set; }
        public int AdminUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class RejectCreditRequestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
