using System;
using MediatR;
using TutorPlatform.Application.Contracts.Subjects;

namespace TutorPlatform.Application.Features.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommand : IRequest<SubjectDto>
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
