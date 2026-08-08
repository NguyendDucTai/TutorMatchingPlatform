using System;
using MediatR;
using TutorPlatform.Application.Contracts.Subjects;

namespace TutorPlatform.Application.Features.Subjects.Commands.ReactivateSubject
{
    public class ReactivateSubjectCommand : IRequest<SubjectDto>
    {
        public Guid Id { get; set; }

        public ReactivateSubjectCommand(Guid id)
        {
            Id = id;
        }
    }
}
