using System;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Credits.Queries.GetCreditTransactions
{
    public class CreditTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? ReferenceId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
