using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class Complaint : BaseEntity
    {
        public int? ReporterId { get; set; }
        public int ReportedUserId { get; set; }
        public int? SessionId { get; set; }
        
        public ComplaintType Type { get; set; }
        public ComplaintSource Source { get; set; }
        public string Description { get; set; } = string.Empty;
        
        public ComplaintStatus Status { get; set; }
        public ComplaintAction ResolutionAction { get; set; }
        public string? ResolutionReason { get; set; }

        // Navigations
        public User? Reporter { get; set; }
        public User ReportedUser { get; set; } = null!;
        public Session? Session { get; set; }
    }
}
