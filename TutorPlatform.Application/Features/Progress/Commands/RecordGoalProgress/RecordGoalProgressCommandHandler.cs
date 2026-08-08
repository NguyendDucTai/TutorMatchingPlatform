using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Progress.Commands.RecordGoalProgress
{
    public class RecordGoalProgressCommandHandler : IRequestHandler<RecordGoalProgressCommand, bool>
    {
        private readonly ILearningGoalRepository _learningGoalRepository;

        public RecordGoalProgressCommandHandler(ILearningGoalRepository learningGoalRepository)
        {
            _learningGoalRepository = learningGoalRepository;
        }

        public async Task<bool> Handle(RecordGoalProgressCommand request, CancellationToken cancellationToken)
        {
            var goal = await _learningGoalRepository.GetByIdAsync(request.GoalId);
            if (goal == null)
            {
                throw new NotFoundException(nameof(LearningGoal), request.GoalId);
            }

            // Verify owner
            if (goal.TutorId != request.TutorId)
            {
                throw new ForbiddenException("You do not have permission to record progress on this goal.");
            }

            goal.AddProgress(request.ProgressPercentage, request.Notes);

            await _learningGoalRepository.UpdateAsync(goal);

            return true;
        }
    }
}
