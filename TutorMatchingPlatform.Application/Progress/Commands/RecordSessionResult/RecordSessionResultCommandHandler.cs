using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Progress.Commands.RecordSessionResult
{
    public class RecordSessionResultCommandHandler : IRequestHandler<RecordSessionResultCommand, RecordSessionResultResult>
    {
        private readonly IAppDbContext _context;

        public RecordSessionResultCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<RecordSessionResultResult> Handle(RecordSessionResultCommand request, CancellationToken cancellationToken)
        {
            // 1. Load session with all needed navigation properties
            var session = await _context.Sessions
                .Include(s => s.Tutor)
                    .ThenInclude(tp => tp.User)
                .Include(s => s.Student)
                    .ThenInclude(sp => sp.User)
                .SingleOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
                return new RecordSessionResultResult { Success = false, Message = "Session not found." };

            // 2. Verify the caller is the tutor of this session
            if (session.Tutor.User.Id != request.TutorUserId)
                return new RecordSessionResultResult { Success = false, Message = "Unauthorized: You are not the tutor of this session." };

            // 3. Only allow recording once session is Completed (AC: "Only available once session status is Completed")
            if (session.Status != SessionStatus.Completed)
                return new RecordSessionResultResult { Success = false, Message = "Session must be Completed before recording a result." };

            // 4. MSG02: Block saving if score is blank AND no goal selected
            bool hasScore = request.Score.HasValue;
            bool hasGoal = request.MilestoneId.HasValue;
            if (!hasScore && !hasGoal)
                return new RecordSessionResultResult { Success = false, Message = "MSG02" };

            // 5. Update session score and tutor comment
            if (request.Score.HasValue)
                session.Score = request.Score;

            if (!string.IsNullOrWhiteSpace(request.TutorComment))
                session.TutorComment = request.TutorComment;

            // 6. Update Milestone completion % and auto-complete at 100%
            bool goalAutoCompleted = false;
            if (hasGoal && request.CompletionPercentage.HasValue)
            {
                var milestone = await _context.LearningMilestones
                    .SingleOrDefaultAsync(m => m.Id == request.MilestoneId!.Value, cancellationToken);

                if (milestone == null)
                    return new RecordSessionResultResult { Success = false, Message = "Milestone not found." };

                milestone.CompletionPercentage = request.CompletionPercentage.Value;

                // Auto-mark In Progress if not started
                if (milestone.Status == MilestoneStatus.NotStarted && request.CompletionPercentage.Value > 0)
                    milestone.Status = MilestoneStatus.InProgress;

                // Auto-complete at 100%
                if (request.CompletionPercentage.Value == 100)
                {
                    milestone.Status = MilestoneStatus.Completed;
                    goalAutoCompleted = true;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new RecordSessionResultResult
            {
                Success = true,
                Message = "Session result recorded successfully. (MSG03)",
                GoalAutoCompleted = goalAutoCompleted
            };
        }
    }
}
