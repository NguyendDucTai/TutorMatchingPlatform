using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Contracts.Subjects;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, SubjectDto>
    {
        private readonly ISubjectRepository _subjectRepository;

        public CreateSubjectCommandHandler(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<SubjectDto> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
        {
            // Ideally we should check if subject name already exists here.
            var existing = await _subjectRepository.GetByNameAsync(request.Name);
            if (existing != null)
            {
                throw new ConflictException($"Subject '{request.Name}' already exists.");
            }

            var subject = new Subject(request.Name, request.Description, null);
            await _subjectRepository.AddAsync(subject);

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
