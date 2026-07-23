using System;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Domain.Entities
{
    public class CreditRequest : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public decimal Amount { get; set; }
        public CreditRequestStatus Status { get; set; } = CreditRequestStatus.Pending;
        public DateTime? ProcessedAt { get; set; }
        public int? ProcessedBy { get; set; }
        public string? Note { get; set; }
    }
}
