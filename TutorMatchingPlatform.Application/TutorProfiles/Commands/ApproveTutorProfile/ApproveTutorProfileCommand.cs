using MediatR;

namespace TutorMatchingPlatform.Application.TutorProfiles.Commands.ApproveTutorProfile
{
    public class ApproveTutorProfileCommand : IRequest<bool>
    {
        public int TutorProfileId { get; set; }
    }
}
