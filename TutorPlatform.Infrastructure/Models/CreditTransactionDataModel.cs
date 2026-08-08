using System;

namespace TutorPlatform.Infrastructure.Models
{
    public class CreditTransactionDataModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public int Type { get; set; }
        public string Description { get; set; } = null!;
        public Guid? BookingId { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDataModel User { get; set; } = null!;
        public BookingDataModel? Booking { get; set; }
    }
}
