using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Bookings.Commands.CompleteBooking
{
    public class CompleteBookingCommandHandler : IRequestHandler<CompleteBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICreditService _creditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public CompleteBookingCommandHandler(
            IBookingRepository bookingRepository,
            ICreditService creditService,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _bookingRepository = bookingRepository;
            _creditService = creditService;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<bool> Handle(CompleteBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException(nameof(Booking), request.BookingId);

            if (booking.TutorId != request.TutorId) throw new ForbiddenException("You can only complete your own bookings.");

            // Prevent completing a booking before the scheduled start time (supports both UTC-stored new bookings and local-stored old bookings)
            var scheduledStart = booking.ScheduledStartAt;
            bool isFutureUtc = DateTime.UtcNow < DateTime.SpecifyKind(scheduledStart, DateTimeKind.Utc);
            bool isFutureLocal = DateTime.Now < DateTime.SpecifyKind(scheduledStart, DateTimeKind.Local);
            if (isFutureUtc && isFutureLocal)
            {
                throw new BadRequestException("Không thể hoàn thành buổi học trước thời gian bắt đầu.");
            }

            booking.Complete();

            // Bug #7: Wrap transfer + update in a single transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _creditService.TransferAsync(booking.StudentId, booking.TutorId, booking.CreditAmount, "Payment for completed session", booking.Id);
                await _bookingRepository.UpdateAsync(booking);
                
                var tutorProfile = await _userRepository.GetTutorProfileAsync(booking.TutorId);
                if (tutorProfile != null)
                {
                    tutorProfile.IncrementSessions();
                    await _userRepository.UpdateTutorProfileAsync(tutorProfile);
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            // Fetch tutor name and publish booking completed event
            var tutor = await _userRepository.GetByIdAsync(booking.TutorId);
            var tutorName = tutor?.FullName ?? "Gia sư";

            await _publisher.Publish(new TutorPlatform.Application.Features.Notifications.Events.BookingCompletedEvent
            {
                Booking = booking,
                TutorName = tutorName
            }, cancellationToken);

            return true;
        }
    }
}
