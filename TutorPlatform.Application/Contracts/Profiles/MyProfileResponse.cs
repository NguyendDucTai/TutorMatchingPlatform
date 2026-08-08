using System;

namespace TutorPlatform.Application.Contracts.Profiles
{
    public class MyProfileResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Role { get; set; } // 1 for Tutor, 2 for Student
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        
        // Only one of these will be populated depending on Role
        public TutorProfileDto? TutorProfile { get; set; }
        public StudentProfileDto? StudentProfile { get; set; }
    }
}
