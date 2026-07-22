using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Subjects.Commands.DeleteSubject
{
    public class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand, DeleteSubjectResult>
    {
        private readonly IAppDbContext _context;

        public DeleteSubjectCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<DeleteSubjectResult> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
        {
            var subject = await _context.Subjects.FindAsync(new object[] { request.Id }, cancellationToken);

            if (subject == null)
            {
                return new DeleteSubjectResult { Success = false, Message = "Subject not found." };
            }

            // Check if any tutor is currently teaching this subject
            var searchString = $"\"SubjectId\":{request.Id}";
            var searchString2 = $"\"SubjectId\": {request.Id}";
            var isSubjectInUse = await _context.TutorProfiles
                .AnyAsync(tp => tp.SubjectsJson != null && (tp.SubjectsJson.Contains(searchString) || tp.SubjectsJson.Contains(searchString2)), cancellationToken);
                
            if (isSubjectInUse)
            {
                return new DeleteSubjectResult { Success = false, Message = "Cannot delete subject because it is being taught by one or more tutors." };
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync(cancellationToken);

            return new DeleteSubjectResult { Success = true, Message = "Subject deleted successfully." };
        }
    }
}
