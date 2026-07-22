using MediatR;

namespace TutorMatchingPlatform.Application.Subjects.Commands.UpdateSubject
{
    public class UpdateSubjectCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
