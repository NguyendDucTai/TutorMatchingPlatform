using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Application.Subjects.Commands.UpdateSubject
{
    public class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand, bool>
    {
        private readonly IAppDbContext _context;

        public UpdateSubjectCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
        {
            var subject = await _context.Subjects.SingleOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
            if (subject == null)
            {
                throw new Exception("Subject not found.");
            }

            var exists = await _context.Subjects.AnyAsync(s => s.Id != request.Id && s.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            if (exists)
            {
                throw new Exception("Subject Name already exists.");
            }

            subject.Name = request.Name;
            subject.Description = request.Description;
            subject.IsActive = request.IsActive;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
