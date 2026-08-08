using System;
using System.Collections.Generic;

namespace TutorPlatform.Application.Contracts.Profiles
{
    public class TutorProfileDto
    {
        public string? Bio { get; set; }
        public string? Qualifications { get; set; }
        public bool IsApproved { get; set; }
        public int ApprovalStatus { get; set; }
        public string? DefaultMeetingLink { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalSessions { get; set; }
        public List<TutorSubjectDto> Subjects { get; set; } = new List<TutorSubjectDto>();
    }

    public class TutorSubjectDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int ProficiencyLevel { get; set; }
        public decimal HourlyCredits { get; set; }
    }
}
