using System;
using MediatR;

namespace TutorPlatform.Application.Features.Admin.Commands.RejectTutor
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
