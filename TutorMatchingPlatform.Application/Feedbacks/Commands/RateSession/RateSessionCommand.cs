using MediatR;

namespace TutorMatchingPlatform.Application.Feedbacks.Commands.RateSession
{
    public class RateSessionCommand : IRequest<RateSessionResult>
    {
        public int SessionId { get; set; }
        public int SenderUserId { get; set; } // Derived from Token claims
        
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
    }
}
