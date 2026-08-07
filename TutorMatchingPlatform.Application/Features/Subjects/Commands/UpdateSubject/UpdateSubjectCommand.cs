using System;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Subjects;

namespace TutorMatchingPlatform.Application.Features.Subjects.Commands.UpdateSubject
{
    public class UpdateSubjectCommand : IRequest<SubjectDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; }
    }
}
