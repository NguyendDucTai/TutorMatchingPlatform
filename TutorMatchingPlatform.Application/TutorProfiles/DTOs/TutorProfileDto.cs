using System;

namespace TutorMatchingPlatform.Application.TutorProfiles.DTOs
{
    public class TutorProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? SubjectsJson { get; set; }
        public string? Qualifications { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
