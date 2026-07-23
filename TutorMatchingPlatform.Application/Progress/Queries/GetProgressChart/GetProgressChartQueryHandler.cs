using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Progress.Queries.GetProgressChart
{
    public class GetProgressChartQueryHandler : IRequestHandler<GetProgressChartQuery, ProgressChartResult>
    {
        private readonly IAppDbContext _context;

        public GetProgressChartQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ProgressChartResult> Handle(GetProgressChartQuery request, CancellationToken cancellationToken)
        {
            DateTime? startDate = request.TimeRange?.ToLower() switch
            {
                "7days" => DateTime.UtcNow.AddDays(-7),
                "30days" => DateTime.UtcNow.AddDays(-30),
                "90days" => DateTime.UtcNow.AddDays(-90),
                _ => null // "all" or invalid
            };

            var query = _context.Sessions
                .Where(s => s.SubjectId == request.SubjectId && s.Status == SessionStatus.Completed)
                .Where(s => s.Student.User.Id == request.UserId || s.Tutor.User.Id == request.UserId);

            if (startDate.HasValue)
            {
                query = query.Where(s => s.StartTime >= startDate.Value);
            }

            var sessions = await query
                .OrderBy(s => s.StartTime)
                .Select(s => new ProgressDataPoint
                {
                    SessionDate = s.StartTime,
                    Score = s.Score,
                    GoalCompletionPercentage = s.GoalCompletionPercentage
                })
                .ToListAsync(cancellationToken);

            if (sessions.Count == 0)
            {
                return new ProgressChartResult
                {
                    HasData = false,
                    Message = "MSG01"
                };
            }

            return new ProgressChartResult
            {
                HasData = true,
                DataPoints = sessions
            };
        }
    }
}
