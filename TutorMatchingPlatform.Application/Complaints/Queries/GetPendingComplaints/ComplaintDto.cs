using System;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Complaints.Queries.GetPendingComplaints
{
    public class ComplaintDto
    {
        public int Id { get; set; }
        public int? ReporterId { get; set; }
        public string? ReporterName { get; set; }
        public int ReportedUserId { get; set; }
        public string ReportedUserName { get; set; } = string.Empty;
        public int? SessionId { get; set; }
        
        public ComplaintType Type { get; set; }
        public ComplaintSource Source { get; set; }
        public string Description { get; set; } = string.Empty;
        
        public ComplaintStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
