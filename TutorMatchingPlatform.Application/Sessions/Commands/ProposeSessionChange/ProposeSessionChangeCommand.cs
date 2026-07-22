using System;
using MediatR;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Sessions.Commands.ProposeSessionChange
{
    public class ProposeSessionChangeCommand : IRequest<ProposeSessionChangeResult>
    {
        public int SessionId { get; set; }
        public int RequesterUserId { get; set; } // Derived from claim
        
        public SessionChangeType ChangeType { get; set; }
        
        // Required if ChangeType is Reschedule
        public DateTime? NewStartTime { get; set; }
        public DateTime? NewEndTime { get; set; }
    }
}
