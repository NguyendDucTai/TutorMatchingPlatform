using System;

namespace TutorMatchingPlatform.Domain.Common
{
    public class PendingTutorResult
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Qualifications { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
