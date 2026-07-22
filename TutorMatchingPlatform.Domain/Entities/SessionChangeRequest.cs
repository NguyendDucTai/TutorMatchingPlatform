using System;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class SessionChangeRequest : BaseEntity
    {
        public int SessionId { get; set; }
        public int RequesterId { get; set; }
        
        public SessionChangeType ChangeType { get; set; }
        public SessionChangeRequestStatus Status { get; set; }
        
        public DateTime? ProposedStartTime { get; set; }
        public DateTime? ProposedEndTime { get; set; }
        
        // Navigation properties
        public Session Session { get; set; } = null!;
        public User Requester { get; set; } = null!;
    }
}
