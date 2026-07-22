using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Application.Progress.Commands.SetLearningGoal
{
    public class SetLearningGoalCommandHandler : IRequestHandler<SetLearningGoalCommand, int>
    {
        private readonly IAppDbContext _context;

        public SetLearningGoalCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(SetLearningGoalCommand request, CancellationToken cancellationToken)
        {
            var tutorUser = await _context.Users
                .Include(u => u.TutorProfile)
                .SingleOrDefaultAsync(u => u.Id == request.TutorId, cancellationToken);

            if (tutorUser == null || tutorUser.TutorProfile == null)
            {
                throw new Exception("Tutor not found.");
            }

            var studentUser = await _context.Users
                .Include(u => u.StudentProfile)
                .SingleOrDefaultAsync(u => u.Id == request.StudentId, cancellationToken);

            if (studentUser == null || studentUser.StudentProfile == null)
            {
                throw new Exception("Student not found.");
            }

            var subject = await _context.Subjects
                .SingleOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);

            if (subject == null)
            {
                throw new Exception("Subject not found.");
            }

            // Authorization check: Ensure the tutor is actually teaching this student in this subject
            // We verify this by checking if there's at least one session between them for this subject.
            var hasSession = await _context.Sessions.AnyAsync(s => 
                s.TutorId == tutorUser.TutorProfile.Id && 
                s.StudentId == studentUser.StudentProfile.Id && 
                s.SubjectId == request.SubjectId, 
                cancellationToken);

            if (!hasSession)
            {
                throw new Exception("Unauthorized: You can only set goals for students you are currently teaching in this subject.");
            }

            var milestone = new LearningMilestone
            {
                StudentId = studentUser.StudentProfile.Id,
                SubjectId = request.SubjectId,
                MilestoneName = request.MilestoneName,
                TargetDate = request.TargetDate,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.LearningMilestones.Add(milestone);
            await _context.SaveChangesAsync(cancellationToken);

            return milestone.Id;
        }
    }
}
