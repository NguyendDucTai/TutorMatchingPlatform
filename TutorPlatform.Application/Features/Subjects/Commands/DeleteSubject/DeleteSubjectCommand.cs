using System;
using MediatR;

namespace TutorPlatform.Application.Features.Subjects.Commands.DeleteSubject
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
