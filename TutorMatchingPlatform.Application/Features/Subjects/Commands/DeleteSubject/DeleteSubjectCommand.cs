using System;
using MediatR;

namespace TutorMatchingPlatform.Application.Features.Subjects.Commands.DeleteSubject
{
    public class DeleteSubjectCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public DeleteSubjectCommand(Guid id)
        {
            Id = id;
        }
    }
}
