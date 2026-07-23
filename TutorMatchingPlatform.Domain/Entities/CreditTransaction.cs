using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class CreditTransaction : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public decimal Amount { get; set; }
        public CreditTransactionType Type { get; set; }
        public string? ReferenceId { get; set; }
        public string? Description { get; set; }
    }
}
