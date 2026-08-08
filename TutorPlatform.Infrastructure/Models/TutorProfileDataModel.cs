using System;
using System.Collections.Generic;

namespace TutorPlatform.Infrastructure.Models
{
    public class TutorProfileDataModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Bio { get; set; }
        public string? Qualifications { get; set; }
        public bool IsApproved { get; set; }
        public int ApprovalStatus { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalSessions { get; set; }
        public string? DefaultMeetingLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDataModel User { get; set; } = null!;
        public ICollection<TutorSubjectDataModel> TutorSubjects { get; set; } = new List<TutorSubjectDataModel>();
    }
}
