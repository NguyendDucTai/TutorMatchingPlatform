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
        private readonly IBookingRepository _bookingRepository;

        public DeleteSubjectCommandHandler(ISubjectRepository subjectRepository, IBookingRepository bookingRepository)
        {
            _subjectRepository = subjectRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<bool> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
        {
            var subject = await _subjectRepository.GetByIdAsync(request.Id);
            if (subject == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Subject), request.Id);
            }

            // Check if there are active (Pending or Confirmed) bookings for this subject
            var activeBookingsCount = await _bookingRepository.CountActiveBookingsBySubjectIdAsync(request.Id);
            if (activeBookingsCount > 0)
            {
                throw new BadRequestException($"Không thể khóa môn học này. Hiện đang có {activeBookingsCount} buổi học đang chờ hoặc đang diễn ra (Pending/Confirmed). Vui lòng đợi các buổi học hoàn thành hoặc xử lý trước khi khóa môn.");
            }

            subject.Deactivate();
            await _subjectRepository.UpdateAsync(subject);

            return true;
        }
    }
}
