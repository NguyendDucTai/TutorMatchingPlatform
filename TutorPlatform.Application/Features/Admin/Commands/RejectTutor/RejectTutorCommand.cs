using System;
using MediatR;

namespace TutorPlatform.Application.Features.Admin.Commands.RejectTutor
{
    public class RejectTutorCommand : IRequest<bool>
    {
        public Guid TutorUserId { get; set; }
        public string? Reason { get; set; }

        public RejectTutorCommand(Guid tutorUserId, string? reason = null)
        {
            TutorUserId = tutorUserId;
            Reason = reason;
        }
    }
}
