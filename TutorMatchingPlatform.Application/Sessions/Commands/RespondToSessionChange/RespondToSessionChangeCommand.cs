using MediatR;

namespace TutorMatchingPlatform.Application.Sessions.Commands.RespondToSessionChange
{
    public class RespondToSessionChangeCommand : IRequest<RespondToSessionChangeResult>
    {
        public int ChangeRequestId { get; set; }
        public int ResponderUserId { get; set; } // Derived from claim
        
        public bool Accept { get; set; }
    }
}
