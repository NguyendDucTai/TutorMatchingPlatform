using System;
using MediatR;

namespace TutorMatchingPlatform.Application.Features.Admin.Commands.RejectTutor
{
    public class RejectTutorCommand : IRequest<bool>
    {
        public Guid TutorUserId { get; set; }

        public RejectTutorCommand(Guid tutorUserId)
        {
            TutorUserId = tutorUserId;
        }
    }
}
