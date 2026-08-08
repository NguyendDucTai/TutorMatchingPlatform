using System;
using MediatR;

namespace TutorPlatform.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
