using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Progress.Commands.UpdateLearningGoal
{
    public class UpdateLearningGoalCommandHandler : IRequestHandler<UpdateLearningGoalCommand, bool>
    {
        private readonly ILearningGoalRepository _learningGoalRepository;

        public UpdateLearningGoalCommandHandler(ILearningGoalRepository learningGoalRepository)
        {
            _learningGoalRepository = learningGoalRepository;
        }

        public async Task<bool> Handle(UpdateLearningGoalCommand request, CancellationToken cancellationToken)
        {
            var goal = await _learningGoalRepository.GetByIdAsync(request.GoalId);
            if (goal == null)
            {
                throw new NotFoundException(nameof(LearningGoal), request.GoalId);
            }

            // Verify owner
            if (goal.TutorId != request.TutorId)
            {
                throw new ForbiddenException("You do not have permission to update this learning goal.");
            }

            goal.UpdateDetails(
                request.Title,
                request.Description,
                request.TargetDate != null ? DateTime.SpecifyKind(request.TargetDate.Value, DateTimeKind.Utc) : null
            );

            await _learningGoalRepository.UpdateAsync(goal);

            return true;
        }
    }
}
