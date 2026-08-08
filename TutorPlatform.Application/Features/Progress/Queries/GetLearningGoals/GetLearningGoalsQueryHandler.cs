using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Contracts.Progress;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Progress.Queries.GetLearningGoals
{
    public class GetLearningGoalsQueryHandler : IRequestHandler<GetLearningGoalsQuery, List<LearningGoalDto>>
    {
        private readonly ILearningGoalRepository _learningGoalRepository;

        public GetLearningGoalsQueryHandler(ILearningGoalRepository learningGoalRepository)
        {
            _learningGoalRepository = learningGoalRepository;
        }

        public async Task<List<LearningGoalDto>> Handle(GetLearningGoalsQuery request, CancellationToken cancellationToken)
        {
            Guid targetStudentId;
            IReadOnlyList<LearningGoal> goals;

            if (request.Role == UserRole.Student)
            {
                targetStudentId = request.CurrentUserId;
                goals = await _learningGoalRepository.GetByStudentIdAsync(targetStudentId);
            }
            else if (request.Role == UserRole.Tutor)
            {
                if (!request.StudentId.HasValue)
                {
                    throw new BadRequestException("StudentId is required for tutors.");
                }
                targetStudentId = request.StudentId.Value;
                // Fetch goals created by this tutor for this student
                goals = await _learningGoalRepository.GetByTutorAndStudentAsync(request.CurrentUserId, targetStudentId);
            }
            else // Admin (if needed, fetch by StudentId)
            {
                if (!request.StudentId.HasValue)
                {
                    throw new BadRequestException("StudentId is required.");
                }
                targetStudentId = request.StudentId.Value;
                goals = await _learningGoalRepository.GetByStudentIdAsync(targetStudentId);
            }

            // Filter by SubjectId if provided
            if (request.SubjectId.HasValue)
            {
                goals = goals.Where(g => g.SubjectId == request.SubjectId.Value).ToList();
            }

            var today = DateTime.UtcNow.Date;

            return goals.Select(g =>
            {
                // Dynamic Overdue status mapping
                var status = (int)g.Status;
                if (g.Status != GoalStatus.Completed && g.TargetDate.HasValue && g.TargetDate.Value.Date < today)
                {
                    status = (int)GoalStatus.Overdue;
                }

                // Get current progress percentage from the latest record
                var latestProgress = g.GoalProgresses.OrderByDescending(gp => gp.RecordedAt).FirstOrDefault();
                var currentProgress = latestProgress?.ProgressPercentage ?? 0;

                return new LearningGoalDto
                {
                    Id = g.Id,
                    TutorId = g.TutorId,
                    StudentId = g.StudentId,
                    SubjectId = g.SubjectId,
                    Title = g.Title,
                    Description = g.Description,
                    TargetDate = g.TargetDate,
                    Status = status,
                    CurrentProgress = currentProgress,
                    CreatedAt = g.CreatedAt
                };
            }).ToList();
        }
    }
}
