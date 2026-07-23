using MediatR;
using System;
using System.Collections.Generic;

namespace TutorMatchingPlatform.Application.Progress.Queries.GetProgressChart
{
    public class GetProgressChartQuery : IRequest<ProgressChartResult>
    {
        public int UserId { get; set; }
        public int SubjectId { get; set; }
        public string TimeRange { get; set; } = "30days"; // 7days, 30days, 90days, all
    }

    public class ProgressChartResult
    {
        public bool HasData { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ProgressDataPoint> DataPoints { get; set; } = new();
    }

    public class ProgressDataPoint
    {
        public DateTime SessionDate { get; set; }
        public double? Score { get; set; }
        public int? GoalCompletionPercentage { get; set; }
    }
}
