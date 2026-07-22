using MediatR;
using TutorMatchingPlatform.Application.Subjects.DTOs;

namespace TutorMatchingPlatform.Application.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommand : IRequest<SubjectDto>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
