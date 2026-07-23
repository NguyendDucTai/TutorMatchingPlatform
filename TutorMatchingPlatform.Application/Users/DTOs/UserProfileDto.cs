namespace TutorMatchingPlatform.Application.Users.DTOs
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsSuspended { get; set; }

        // Tutor specific fields
        public string? Bio { get; set; }
        public string? Qualifications { get; set; }
        public string? SubjectsJson { get; set; }
        public string? FreeSchedulesJson { get; set; }
        public string? TimezoneOffset { get; set; }
        public string? TutorStatus { get; set; }

        // Student specific fields
        public string? StudyGoals { get; set; }
        public string? TargetSubjectsJson { get; set; }
    }
}
