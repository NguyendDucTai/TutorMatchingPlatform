using MediatR;

namespace TutorMatchingPlatform.Application.Credits.Commands.ApproveCreditRequest
{
    public class ApproveCreditRequestCommand : IRequest<ApproveCreditRequestResult>
    {
        public int CreditRequestId { get; set; }
        public int AdminUserId { get; set; }
    }

    public class ApproveCreditRequestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
