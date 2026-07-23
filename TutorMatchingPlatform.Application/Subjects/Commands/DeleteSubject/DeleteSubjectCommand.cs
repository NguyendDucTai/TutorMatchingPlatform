using MediatR;

namespace TutorMatchingPlatform.Application.Subjects.Commands.DeleteSubject
{
    public class DeleteSubjectCommand : IRequest<DeleteSubjectResult>
    {
        public int Id { get; set; }
    }
}
