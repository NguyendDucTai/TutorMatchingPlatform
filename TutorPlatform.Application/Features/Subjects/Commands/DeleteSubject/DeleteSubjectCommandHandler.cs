using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Subjects.Commands.DeleteSubject
{
    public class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand, bool>
    {
        private readonly ISubjectRepository _subjectRepository;

        public DeleteSubjectCommandHandler(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<bool> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
        {
            var subject = await _subjectRepository.GetByIdAsync(request.Id);
            if (subject == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Subject), request.Id);
            }

            subject.Deactivate();
            await _subjectRepository.UpdateAsync(subject);

            return true;
        }
    }
}
