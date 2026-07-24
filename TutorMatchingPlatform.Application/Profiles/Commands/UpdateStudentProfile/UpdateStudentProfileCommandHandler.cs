using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Profiles.Commands.UpdateStudentProfile
{
    public class UpdateStudentProfileCommandHandler : IRequestHandler<UpdateStudentProfileCommand, bool>
    {
        private readonly IAppDbContext _context;

        public UpdateStudentProfileCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateStudentProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.StudentProfile)
                .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null || user.Role != UserRole.Student || user.StudentProfile == null)
            {
                return false;
            }

            if (request.StudyGoals != null)
            {
                user.StudentProfile.StudyGoals = request.StudyGoals;
            }
            if (request.TargetSubjectsJson != null)
            {
                user.StudentProfile.TargetSubjectsJson = request.TargetSubjectsJson;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
