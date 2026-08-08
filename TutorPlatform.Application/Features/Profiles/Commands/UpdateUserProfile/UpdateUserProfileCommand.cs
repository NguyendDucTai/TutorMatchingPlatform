using System;
using MediatR;

namespace TutorPlatform.Application.Features.Profiles.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
