using MediatR;

namespace TutorMatchingPlatform.Application.TutorProfiles.Commands.RejectTutorProfile
{
    public class RejectTutorProfileCommand : IRequest<bool>
    {
        public int TutorProfileId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
