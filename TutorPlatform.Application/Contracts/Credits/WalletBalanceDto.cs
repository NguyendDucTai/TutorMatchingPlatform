using System;

namespace TutorPlatform.Application.Contracts.Credits
{
    public class WalletBalanceDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public decimal CreditBalance { get; set; }
    }
}
