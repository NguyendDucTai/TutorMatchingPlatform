using System;
using MediatR;

namespace TutorPlatform.Application.Features.Tutors.Commands.ApproveTutor
{
    public class ApproveTutorCommand : IRequest<bool>
    {
        public Guid TutorUserId { get; set; }
        public Guid AdminUserId { get; set; }
    }
}
