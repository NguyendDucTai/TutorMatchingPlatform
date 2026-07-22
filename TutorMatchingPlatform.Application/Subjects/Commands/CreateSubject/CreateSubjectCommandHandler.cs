using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Subjects.DTOs;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Application.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, SubjectDto>
    {
        private readonly IAppDbContext _context;

        public CreateSubjectCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<SubjectDto> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
        {
            var exists = await _context.Subjects.AnyAsync(s => s.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            if (exists)
            {
                throw new Exception("Subject Name already exists.");
            }

            var subject = new Subject
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true
            };

            await _context.Subjects.AddAsync(subject, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new SubjectDto
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description,
                IsActive = subject.IsActive
            };
        }
    }
}
