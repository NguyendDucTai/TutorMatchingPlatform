using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsSuspended { get; set; }

        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation properties
        public TutorProfile? TutorProfile { get; set; }
        public StudentProfile? StudentProfile { get; set; }
    }
}
