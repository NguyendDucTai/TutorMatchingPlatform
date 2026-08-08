using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Contracts.Subjects;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Subjects.Commands.ReactivateSubject
{
    public class ReactivateSubjectCommandHandler : IRequestHandler<ReactivateSubjectCommand, SubjectDto>
    {
        private readonly ISubjectRepository _subjectRepository;

        public ReactivateSubjectCommandHandler(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<SubjectDto> Handle(ReactivateSubjectCommand request, CancellationToken cancellationToken)
        {
            var subject = await _subjectRepository.GetByIdAsync(request.Id);
            if (subject == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Subject), request.Id);
            }

            subject.Activate();
            await _subjectRepository.UpdateAsync(subject);

            return subject.Adapt<SubjectDto>();
        }
    }
}
