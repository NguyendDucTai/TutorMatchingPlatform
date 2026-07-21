using TutorMatchingPlatform.Domain.Common;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class Feedback : BaseEntity
    {
        public int SessionId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }

        // Navigation properties
        public Session Session { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }
}
