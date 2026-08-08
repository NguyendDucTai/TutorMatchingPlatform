using System.Collections.Generic;

namespace TutorPlatform.Domain.Common
{
    public class AdminRevenueReport
    {
        public decimal TotalRevenue { get; set; }
        public List<AdminRevenuePeriod> Periods { get; set; } = new();
    }

    public class AdminRevenuePeriod
    {
        public string PeriodName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
    }
}
