using System;
using System.Collections.Generic;

namespace TutorPlatform.Application.Contracts.Progress
{
    public class ProgressChartDto
    {
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public List<GoalChartDto> Goals { get; set; } = new();
        public List<SessionScoreDto> SessionScores { get; set; } = new();
    }

    public class GoalChartDto
    {
        public Guid GoalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public List<ProgressHistoryDto> ProgressHistory { get; set; } = new();
    }

    public class ProgressHistoryDto
    {
        public DateTime RecordedAt { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class SessionScoreDto
    {
        public DateTime Date { get; set; }
        public decimal Score { get; set; }
    }
}
