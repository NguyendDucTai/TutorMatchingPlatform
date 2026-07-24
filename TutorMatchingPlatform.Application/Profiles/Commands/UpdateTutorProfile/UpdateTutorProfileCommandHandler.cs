using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Profiles.Commands.UpdateTutorProfile
{
    public class UpdateTutorProfileCommandHandler : IRequestHandler<UpdateTutorProfileCommand, bool>
    {
        private readonly IAppDbContext _context;

        public UpdateTutorProfileCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateTutorProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.TutorProfile)
                .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null || user.Role != UserRole.Tutor || user.TutorProfile == null)
            {
                return false;
            }

            if (request.Bio != null)
            {
                user.TutorProfile.Bio = request.Bio;
            }

            if (request.QualificationsText != null)
            {
                user.TutorProfile.Qualifications = request.QualificationsText;
            }

            // Setting back to Pending if approved so Admin can review new changes
            if (user.TutorProfile.Status == ProfileStatus.Approved)
            {
                user.TutorProfile.Status = ProfileStatus.Pending;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
