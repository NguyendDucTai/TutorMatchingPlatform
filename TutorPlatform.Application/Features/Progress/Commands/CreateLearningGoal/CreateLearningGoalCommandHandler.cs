using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Progress.Commands.CreateLearningGoal
{
    public class CreateLearningGoalCommandHandler : IRequestHandler<CreateLearningGoalCommand, Guid>
    {
        private readonly ILearningGoalRepository _learningGoalRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISubjectRepository _subjectRepository;

        public CreateLearningGoalCommandHandler(
            ILearningGoalRepository learningGoalRepository,
            IUserRepository userRepository,
            ISubjectRepository subjectRepository)
        {
            _learningGoalRepository = learningGoalRepository;
            _userRepository = userRepository;
            _subjectRepository = subjectRepository;
        }

        public async Task<Guid> Handle(CreateLearningGoalCommand request, CancellationToken cancellationToken)
        {
            // Verify Student exists
            var student = await _userRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                throw new NotFoundException(nameof(User), request.StudentId);
            }

            if (student.Role != TutorPlatform.Domain.Enums.UserRole.Student)
            {
                throw new BadRequestException("The specified user is not a Student.");
            }

            // Verify Subject exists
            var subject = await _subjectRepository.GetByIdAsync(request.SubjectId);
            if (subject == null)
            {
                throw new NotFoundException(nameof(Subject), request.SubjectId);
            }

            // Create Learning Goal
            var goal = new LearningGoal(
                request.TutorId,
                request.StudentId,
                request.SubjectId,
                request.Title,
                request.Description,
                request.TargetDate != null ? DateTime.SpecifyKind(request.TargetDate.Value, DateTimeKind.Utc) : null
            );

            await _learningGoalRepository.AddAsync(goal);

            return goal.Id;
        }
    }
}
