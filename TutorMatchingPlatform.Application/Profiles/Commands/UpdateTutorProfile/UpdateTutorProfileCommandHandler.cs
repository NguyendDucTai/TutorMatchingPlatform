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

            var hasReviewableChanges = false;

            if (request.Bio != null && request.Bio != user.TutorProfile.Bio)
            {
                user.TutorProfile.Bio = request.Bio;
                hasReviewableChanges = true;
            }

            if (request.QualificationsText != null &&
                request.QualificationsText != user.TutorProfile.Qualifications)
            {
                user.TutorProfile.Qualifications = request.QualificationsText;
                hasReviewableChanges = true;
            }

            // Only changed reviewable content needs another admin review.
            if (hasReviewableChanges && user.TutorProfile.Status == ProfileStatus.Approved)
            {
                user.TutorProfile.Status = ProfileStatus.Pending;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
