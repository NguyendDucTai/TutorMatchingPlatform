using MediatR;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Complaints.Commands.SubmitComplaint
{
    public class SubmitComplaintCommand : IRequest<SubmitComplaintResult>
    {
        public int ReporterId { get; set; } // Derived from auth claim
        public int ReportedUserId { get; set; }
        public int? SessionId { get; set; }
        
        public ComplaintType Type { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
