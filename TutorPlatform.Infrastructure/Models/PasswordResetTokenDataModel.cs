using System;

namespace TutorPlatform.Infrastructure.Models
{
    public class PasswordResetTokenDataModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public UserDataModel User { get; set; } = null!;
    }
}
