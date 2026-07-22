using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Application.Complaints.Commands.ResolveComplaint
{
    public class ResolveComplaintCommandHandler : IRequestHandler<ResolveComplaintCommand, ResolveComplaintResult>
    {
        private readonly IAppDbContext _context;

        public ResolveComplaintCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ResolveComplaintResult> Handle(ResolveComplaintCommand request, CancellationToken cancellationToken)
        {
            var complaint = await _context.Complaints
                .Include(c => c.ReportedUser)
                .SingleOrDefaultAsync(c => c.Id == request.ComplaintId, cancellationToken);

            if (complaint == null)
            {
                return new ResolveComplaintResult { Success = false, Message = "Complaint not found." };
            }

            if (complaint.Status != ComplaintStatus.Pending)
            {
                return new ResolveComplaintResult { Success = false, Message = "Complaint is already resolved or closed." };
            }

            complaint.ResolutionAction = request.Action;
            complaint.ResolutionReason = request.Reason;
            
            if (request.Action == ComplaintAction.Close)
            {
                complaint.Status = ComplaintStatus.Closed;
            }
            else
            {
                complaint.Status = ComplaintStatus.Resolved;

                if (request.Action == ComplaintAction.TemporarySuspension)
                {
                    // BR-08: Suspension requires period + reason and blocks login
                    var days = request.SuspendDays ?? 7;
                    var lockEnd = DateTime.UtcNow.AddDays(days);
                    
                    // If user is already locked out further in the future, extend it or just set it
                    if (!complaint.ReportedUser.LockoutEnd.HasValue || complaint.ReportedUser.LockoutEnd.Value < lockEnd)
                    {
                        complaint.ReportedUser.LockoutEnd = lockEnd;
                    }
                    
                    // Force refresh token invalidation
                    complaint.ReportedUser.RefreshToken = null;
                    complaint.ReportedUser.RefreshTokenExpiryTime = null;
                }
            }

            // Create notification for reporter (MSG11)
            if (complaint.ReporterId.HasValue)
            {
                var notification = new Notification
                {
                    ReceiverId = complaint.ReporterId.Value,
                    Title = "Complaint Resolved",
                    Message = $"Your complaint against {complaint.ReportedUser.FullName} has been reviewed and resolved.",
                    IsWarning = false,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }
            
            // Create notification for reported user if action taken
            if (request.Action == ComplaintAction.Warning || request.Action == ComplaintAction.TemporarySuspension)
            {
                var actionText = request.Action == ComplaintAction.TemporarySuspension ? "suspended" : "warned";
                var notification = new Notification
                {
                    ReceiverId = complaint.ReportedUserId,
                    Title = "Admin Action Taken",
                    Message = $"You have been {actionText} by an Administrator. Reason: {request.Reason}",
                    IsWarning = true,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new ResolveComplaintResult
            {
                Success = true,
                Message = "Complaint resolved successfully."
            };
        }
    }
}
