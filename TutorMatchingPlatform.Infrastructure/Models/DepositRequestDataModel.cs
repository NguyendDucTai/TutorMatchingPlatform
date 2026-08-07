using System;

namespace TutorMatchingPlatform.Infrastructure.Models
{
    public class DepositRequestDataModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; } // 0 = Pending, 1 = Approved, 2 = Rejected
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public UserDataModel User { get; set; } = null!;
    }
}
