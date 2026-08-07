using System;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Subjects;

namespace TutorMatchingPlatform.Application.Features.Subjects.Commands.ReactivateSubject
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
