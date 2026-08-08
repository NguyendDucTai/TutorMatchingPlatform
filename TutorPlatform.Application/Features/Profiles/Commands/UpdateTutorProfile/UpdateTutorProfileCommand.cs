using System;
using MediatR;

namespace TutorPlatform.Application.Features.Profiles.Commands.UpdateTutorProfile
{
    public class UpdateTutorProfileCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string? Bio { get; set; }
        public string? Qualifications { get; set; }
        public string? DefaultMeetingLink { get; set; }
    }
}
