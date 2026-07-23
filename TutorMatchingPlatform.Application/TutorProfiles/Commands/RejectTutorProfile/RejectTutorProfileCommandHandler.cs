using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.TutorProfiles.Commands.RejectTutorProfile
{
    public class RejectTutorProfileCommandHandler : IRequestHandler<RejectTutorProfileCommand, bool>
    {
        private readonly IAppDbContext _context;

        public RejectTutorProfileCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(RejectTutorProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _context.Users
                .Include(u => u.TutorProfile)
                .SingleOrDefaultAsync(u => u.TutorProfile != null && u.TutorProfile.Id == request.TutorProfileId, cancellationToken);

            if (profile == null || profile.TutorProfile == null)
            {
                throw new Exception("Tutor profile not found.");
            }

            if (profile.TutorProfile.Status != ProfileStatus.Pending)
            {
                throw new Exception("Only Pending profiles can be rejected.");
            }

            // Reject
            profile.TutorProfile.Status = ProfileStatus.Rejected;

            // Send MSG07 Notification
            var notification = new Notification
            {
                ReceiverId = profile.Id,
                Title = "Profile Rejected",
                Message = $"Your profile has been rejected. Reason: {request.Reason}.",
                IsRead = false,
                IsWarning = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Log activity
            Console.WriteLine($"[ActivityLog] Administrator rejected TutorProfile {request.TutorProfileId} at {DateTime.UtcNow}. Reason: {request.Reason}");

            return true;
        }
    }
}
