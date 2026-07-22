using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.TutorProfiles.Commands.ApproveTutorProfile
{
    public class ApproveTutorProfileCommandHandler : IRequestHandler<ApproveTutorProfileCommand, bool>
    {
        private readonly IAppDbContext _context;

        public ApproveTutorProfileCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ApproveTutorProfileCommand request, CancellationToken cancellationToken)
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
                throw new Exception("Only Pending profiles can be approved.");
            }

            // Check if required info is missing (MSG02 equivalent blocking)
            if (string.IsNullOrWhiteSpace(profile.TutorProfile.SubjectsJson) || 
                string.IsNullOrWhiteSpace(profile.AvatarUrl))
            {
                throw new Exception("MSG02: Missing required information. Cannot approve.");
            }

            // Approve
            profile.TutorProfile.Status = ProfileStatus.Approved;

            // Send MSG06 Notification
            var notification = new Notification
            {
                ReceiverId = profile.Id,
                Title = "Profile Approved",
                Message = "Your profile has been approved. You may now start accepting students.",
                IsRead = false,
                IsWarning = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // In a real app, you might also use ILogger to log "records the action in the Activity Log" here.
            Console.WriteLine($"[ActivityLog] Administrator approved TutorProfile {request.TutorProfileId} at {DateTime.UtcNow}");

            return true;
        }
    }
}
