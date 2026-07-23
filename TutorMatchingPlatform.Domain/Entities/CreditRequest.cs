using System;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class CreditRequest : BaseEntity
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public CreditRequestStatus Status { get; set; }
        public DateTime? ProcessedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
