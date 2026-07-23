using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Progress.Queries.GetLearningGoals
{
    public class GetLearningGoalsQueryHandler : IRequestHandler<GetLearningGoalsQuery, List<LearningGoalDto>>
    {
        private readonly IAppDbContext _context;

        public GetLearningGoalsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LearningGoalDto>> Handle(GetLearningGoalsQuery request, CancellationToken cancellationToken)
        {
            // The query returns learning goals for a specific subject, where the user is either the student or tutor.
            var isTutor = await _context.Sessions.AnyAsync(s => s.TutorId == request.UserId && s.SubjectId == request.SubjectId, cancellationToken);
            var isStudent = await _context.Sessions.AnyAsync(s => s.StudentId == request.UserId && s.SubjectId == request.SubjectId, cancellationToken);

            var query = _context.LearningMilestones.AsQueryable();

            if (isStudent)
            {
                query = query.Where(g => g.StudentId == request.UserId && g.SubjectId == request.SubjectId);
            }
            else if (isTutor)
            {
                // Tutor viewing student's goals
                // A tutor might have multiple students in the same subject, but this query is simplified.
                // It's better to just get all goals related to the tutor's students in this subject.
                var studentIds = await _context.Sessions
                    .Where(s => s.TutorId == request.UserId && s.SubjectId == request.SubjectId)
                    .Select(s => s.StudentId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                    
                query = query.Where(g => studentIds.Contains(g.StudentId) && g.SubjectId == request.SubjectId);
            }
            else
            {
                // Not involved
                return new List<LearningGoalDto>();
            }

            var goals = await query.OrderBy(g => g.TargetDate).ToListAsync(cancellationToken);

            return goals.Select(g => new LearningGoalDto
            {
                Id = g.Id,
                StudentId = g.StudentId,
                SubjectId = g.SubjectId,
                MilestoneName = g.MilestoneName,
                TargetDate = g.TargetDate,
                Status = g.Status,
                CompletionPercentage = g.CompletionPercentage
            }).ToList();
        }
    }
}
