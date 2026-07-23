using MediatR;
using System;
using System.Collections.Generic;

namespace TutorMatchingPlatform.Application.Admin.Queries.GetDashboardStatistics
{
    public class GetDashboardStatisticsQuery : IRequest<DashboardStatisticsDto>
    {
        public string TimeRange { get; set; } = "30days"; // 7days, 30days, quarter, custom
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
    }

    public class DashboardStatisticsDto
    {
        public int TotalCompletedSessions { get; set; }
        public int TotalCancelledSessions { get; set; }
        public int TotalPendingSessions { get; set; }
        
        public List<PopularSubjectDto> PopularSubjects { get; set; } = new();
        
        public double GoalCompletionRate { get; set; } // Percentage of goals completed
        
        public int PendingTutorApprovals { get; set; }
        public int OpenComplaints { get; set; }
    }

    public class PopularSubjectDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public int SessionCount { get; set; }
    }
}
