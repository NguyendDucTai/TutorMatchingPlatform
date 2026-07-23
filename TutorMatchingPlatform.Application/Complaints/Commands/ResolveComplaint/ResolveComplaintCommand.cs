using MediatR;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Complaints.Commands.ResolveComplaint
{
    public class ResolveComplaintCommand : IRequest<ResolveComplaintResult>
    {
        public int ComplaintId { get; set; }
        public ComplaintAction Action { get; set; }
        public string? Reason { get; set; }
        public int? SuspendDays { get; set; }
    }
}
