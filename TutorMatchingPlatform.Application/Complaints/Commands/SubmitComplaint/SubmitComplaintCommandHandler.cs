using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Complaints.Commands.SubmitComplaint
{
    public class SubmitComplaintCommandHandler : IRequestHandler<SubmitComplaintCommand, SubmitComplaintResult>
    {
        private readonly IAppDbContext _context;

        public SubmitComplaintCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<SubmitComplaintResult> Handle(SubmitComplaintCommand request, CancellationToken cancellationToken)
        {
            var reportedUser = await _context.Users.FindAsync(new object[] { request.ReportedUserId }, cancellationToken);
            if (reportedUser == null)
            {
                return new SubmitComplaintResult { Success = false, Message = "Reported user not found." };
            }

            if (request.SessionId.HasValue)
            {
                var session = await _context.Sessions.FindAsync(new object[] { request.SessionId.Value }, cancellationToken);
                if (session == null)
                {
                    return new SubmitComplaintResult { Success = false, Message = "Session not found." };
                }
            }

            var complaint = new Complaint
            {
                ReporterId = request.ReporterId,
                ReportedUserId = request.ReportedUserId,
                SessionId = request.SessionId,
                Type = request.Type,
                Source = ComplaintSource.UserSubmitted,
                Description = request.Description,
                Status = ComplaintStatus.Pending,
                ResolutionAction = ComplaintAction.None
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync(cancellationToken);

            return new SubmitComplaintResult
            {
                Success = true,
                Message = "Complaint submitted successfully.",
                ComplaintId = complaint.Id
            };
        }
    }
}
