using System;
using System.Collections.Generic;

namespace TutorMatchingPlatform.Domain.Common
{
    public class TutorSearchResult
    {
        public Guid TutorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Qualifications { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalSessions { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? DefaultMeetingLink { get; set; }
        public List<TutorSearchSubjectResult> Subjects { get; set; } = new();
    }

    public class TutorSearchSubjectResult
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int ProficiencyLevel { get; set; }
        public decimal HourlyCredits { get; set; }
    }
}
