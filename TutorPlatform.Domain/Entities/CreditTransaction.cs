using System;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Domain.Entities
{
    public class CreditTransaction : BaseEntity
    {
        public Guid UserId { get; private set; }
        public decimal Amount { get; private set; }
        public CreditTransactionType Type { get; private set; }
        public string Description { get; private set; }
        public Guid? BookingId { get; private set; }
        public decimal BalanceAfter { get; private set; }

        private CreditTransaction() { } // EF Core

        public CreditTransaction(Guid userId, decimal amount, CreditTransactionType type, string description, decimal balanceAfter, Guid? bookingId = null)
        {
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty.");

            UserId = userId;
            Amount = amount;
            Type = type;
            Description = description;
            BalanceAfter = balanceAfter;
            BookingId = bookingId;
        }
    }
}
