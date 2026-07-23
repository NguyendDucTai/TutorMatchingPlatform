using System;

namespace TutorMatchingPlatform.Application.Credits.Queries.GetPendingCreditRequests
{
    public class CreditRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
