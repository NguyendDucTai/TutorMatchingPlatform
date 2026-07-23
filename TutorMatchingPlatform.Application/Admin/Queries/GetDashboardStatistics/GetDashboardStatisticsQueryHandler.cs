using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Admin.Queries.GetDashboardStatistics
{
    public class GetDashboardStatisticsQueryHandler : IRequestHandler<GetDashboardStatisticsQuery, DashboardStatisticsDto>
    {
        private readonly IAppDbContext _context;

        public GetDashboardStatisticsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatisticsDto> Handle(GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            DateTime startDate;

            switch (request.TimeRange.ToLower())
            {
                case "7days":
                    startDate = now.AddDays(-7);
                    break;
                case "quarter":
                    startDate = now.AddMonths(-3);
                    break;
                case "custom":
                    startDate = request.CustomStartDate ?? now.AddDays(-30);
                    now = request.CustomEndDate ?? now;
                    break;
                case "30days":
                default:
                    startDate = now.AddDays(-30);
                    break;
            }

            // 1. Total sessions by status in range
            var sessionsInRange = await _context.Sessions
                .Where(s => s.StartTime >= startDate && s.StartTime <= now)
                .ToListAsync(cancellationToken);

            int completed = sessionsInRange.Count(s => s.Status == SessionStatus.Completed);
            int cancelled = sessionsInRange.Count(s => s.Status == SessionStatus.Cancelled);
            int pending = sessionsInRange.Count(s => s.Status == SessionStatus.Pending || s.Status == SessionStatus.Confirmed); // Depending on definition, we group pending/confirmed here

            // 2. Top 5 popular subjects
            var popularSubjects = sessionsInRange
                .GroupBy(s => s.SubjectId)
                .Select(g => new { SubjectId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var subjectIds = popularSubjects.Select(p => p.SubjectId).ToList();
            var subjects = await _context.Subjects
                .Where(s => subjectIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            var popularSubjectDtos = popularSubjects.Select(p => new PopularSubjectDto
            {
                SubjectName = subjects.FirstOrDefault(s => s.Id == p.SubjectId)?.Name ?? "Unknown",
                SessionCount = p.Count
            }).ToList();

            // 3. Goal completion rate
            var goals = await _context.LearningMilestones
                .Where(g => g.CreatedAt >= startDate && g.CreatedAt <= now)
                .ToListAsync(cancellationToken);

            double goalCompletionRate = 0;
            if (goals.Any())
            {
                int completedGoals = goals.Count(g => g.Status == MilestoneStatus.Completed);
                goalCompletionRate = Math.Round((double)completedGoals / goals.Count * 100, 2);
            }

            // 4. Pending tutor approvals
            int pendingTutors = await _context.TutorProfiles
                .CountAsync(t => t.Status == ProfileStatus.Pending, cancellationToken);

            // 5. Open complaints counts
            int openComplaints = await _context.Complaints
                .CountAsync(c => c.Status == ComplaintStatus.Pending, cancellationToken);

            return new DashboardStatisticsDto
            {
                TotalCompletedSessions = completed,
                TotalCancelledSessions = cancelled,
                TotalPendingSessions = pending,
                PopularSubjects = popularSubjectDtos,
                GoalCompletionRate = goalCompletionRate,
                PendingTutorApprovals = pendingTutors,
                OpenComplaints = openComplaints
            };
        }
    }
}
