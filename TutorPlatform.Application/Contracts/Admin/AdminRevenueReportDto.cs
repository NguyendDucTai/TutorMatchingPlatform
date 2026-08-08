using System.Collections.Generic;

namespace TutorPlatform.Application.Contracts.Admin
{
    public class AdminRevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public List<AdminRevenuePeriodDto> Periods { get; set; } = new();
    }

    public class AdminRevenuePeriodDto
    {
        public string PeriodName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
    }
}
