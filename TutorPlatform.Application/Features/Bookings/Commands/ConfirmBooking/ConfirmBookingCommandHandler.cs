using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Domain.Entities;

namespace TutorPlatform.Application.Features.Bookings.Commands.ConfirmBooking
{
    public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public ConfirmBookingCommandHandler(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<bool> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException(nameof(Booking), request.BookingId);

            if (booking.TutorId != request.TutorId) throw new ForbiddenException("You can only confirm your own bookings.");

            // Check for overlapping confirmed bookings for this tutor
            var tutorBookings = await _bookingRepository.GetByTutorIdAsync(booking.TutorId);
            var hasOverlapConflict = tutorBookings.Any(b => 
                b.Id != booking.Id &&
                b.Status == TutorPlatform.Domain.Enums.BookingStatus.Confirmed &&
                b.ScheduledStartAt < booking.ScheduledEndAt &&
                b.ScheduledEndAt > booking.ScheduledStartAt &&
                (b.ScheduledStartAt != booking.ScheduledStartAt || 
                 b.ScheduledEndAt != booking.ScheduledEndAt || 
                 b.SubjectId != booking.SubjectId)
            );

            if (hasOverlapConflict)
            {
                throw new ConflictException("Bạn hiện đang trong thời gian dạy vui lòng không nhận dạy.");
            }

            booking.Confirm(request.MeetingLink);

            await _bookingRepository.UpdateAsync(booking);

            // Fetch tutor name and publish confirmation notification
            var tutor = await _userRepository.GetByIdAsync(booking.TutorId);
            var tutorName = tutor?.FullName ?? "Gia sư";

            await _publisher.Publish(new TutorPlatform.Application.Features.Notifications.Events.BookingConfirmedEvent
            {
                Booking = booking,
                TutorName = tutorName
            }, cancellationToken);

            return true;
        }
    }
}
