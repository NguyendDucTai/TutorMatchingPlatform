using TutorMatchingPlatform.Domain.Common;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public int ReceiverId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsWarning { get; set; }
        public bool IsRead { get; set; }

        // Navigation properties
        public User Receiver { get; set; } = null!;
    }
}
