using System;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Subjects;

namespace TutorMatchingPlatform.Application.Features.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommand : IRequest<SubjectDto>
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
